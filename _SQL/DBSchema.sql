if exists (select * from sys.databases where name = 'TicTacTotalDomination')
begin
	--Force all connections closed.
	use master
	alter database TicTacTotalDomination set single_user with rollback immediate
	--Drop the database so we can re-create it.
	drop database TicTacTotalDomination
end

create database TicTacTotalDomination
go

use TicTacTotalDomination
go

create table dbo.Player(
	PlayerId int identity(1,1) primary key not null,
	PlayerName varchar(50) not null,
	unique(PlayerName)
)
go

create table dbo.Match(
	MatchId int identity(1,1) primary key not null,
	CreateDate datetime2(7) not null,
	WonDate datetime2(7) null,
	EndDate datetime2(7) null,
	StateDate datetime2(7) not null,
	NumberOfRounds int not null,
	PlayerOneId int foreign key references dbo.Player not null,
	PlayerOneAccepted bit null,
	PlayerTwoId int foreign key references dbo.Player not null,
	PlayerTwoAccepted bit null,
	WinningPlayerId int foreign key references dbo.Player null,
	constraint CK_Match_TwoPlayers check(PlayerOneId <> PlayerTwoId),
	constraint CK_Match_WinnerPlaying check(WinningPlayerId is null
										or WinningPlayerId = PlayerOneId
										or WinningPlayerId = PlayerTwoId),
)
go

create table dbo.ConfigSection(
	SectionId int identity(1,1) primary key not null,
	MatchId int foreign key references dbo.Match not null,
	Section varchar(500) not null
)
go

create table dbo.AuditLog(
	LogId int identity(1,1) primary key not null,
	LogType varchar(50) not null,
	LogDateTime datetime2(7) not null,
	Metadata varchar(250) null
)
go

create table dbo.AuditLogSection(
	SectionId int identity(1,1) primary key not null,
	AuditLogId int foreign key references dbo.AuditLog not null,
	Section varchar(500) not null
)
go

create table dbo.Game(
	GameId int identity(1,1) primary key not null,
	MatchId int foreign key references dbo.Match not null,
	PlayerOneId int foreign key references dbo.Player not null,
	PlayerOneAccepted bit null,
	PlayerTwoId int foreign key references dbo.Player not null,
	PlayerTwoAccepted bit null,
	WinningPlayerId int foreign key references dbo.Player null,
	CurrentPlayerId int foreign key references dbo.Player null,
	StateDate datetime2(7) not null,
	CreateDate datetime2(7) not null,
	WonDate datetime2(7) null,
	EndDate datetime2(7) null,
	DeathMatchMode bit not null, 
	constraint CK_Game_TwoPlayers check(PlayerOneId <> PlayerTwoId),
	constraint CK_Game_WinnerPlaying check(WinningPlayerId is null
										or WinningPlayerId = PlayerOneId
										or WinningPlayerId = PlayerTwoId),
	constraint CK_Game_CurrentPlayerPlaying check(CurrentPlayerId is null
												or CurrentPlayerId = PlayerOneId
												or CurrentPlayerId = PlayerTwoId)
)
go

alter table dbo.Match
add CurrentGameId int foreign key references dbo.Game null
go

create table dbo.AIGame(
	AIGameId int identity(1,1) primary key not null,
	MatchId int foreign key references dbo.Match null,
	GameId int foreign key references dbo.Game not null,
	PlayerId int foreign key references dbo.Player not null,
	EvaluatingMove bit not null,
)
go

create table dbo.GameMove(
	MoveId int identity(1,1) primary key not null,
	GameId int foreign key references dbo.Game not null,
	PlayerId int foreign key references dbo.Player not null,
	MoveDate datetime2(7) not null,
	IsSettingPiece bit not null,
	X int not null,
	y int not null,
)
go

create table dbo.CentralServerSession(
	CentralServerSessionId int identity(1,1) primary key not null,
	CentralServerGameId int null,
	GameId int foreign key references dbo.Game not null,
)
go

--User Creation
if not exists (select name from master.sys.server_principals where name = 'TicTacUser')
begin
	use master
	create login TicTacUser with password = 'T07aLD0m1nat1oN', default_database = TicTacTotalDomination
end

if not exists (select name from TicTacTotalDomination.sys.database_principals where name = 'TicTacUser')
begin
	use TicTacTotalDomination
	create user TicTacUser for login TicTacUser;
end
go

--Stored Procedure Creation
create procedure dbo.sp_GetAIGamesForEvaluation
as
begin
	declare @GamesForEvaluation table (GameId int, PlayerId int);
	
	--Mark the games requiring AI attention as being attended to and retrieve them.
	update ai
	set ai.EvaluatingMove = 1
	output inserted.GameId, inserted.PlayerId into @GamesForEvaluation
	from dbo.AIGame ai
		join dbo.Game gm
			on ai.GameId = gm.GameId
				and ai.PlayerId = gm.CurrentPlayerId
	where gm.WonDate is null
		and gm.WinningPlayerId is null
		and ai.EvaluatingMove = 0;

	select * from @GamesForEvaluation
end
go

create procedure dbo.sp_GetAllLogsForMatch(@matchId int)
as
begin
select lg.*
from dbo.AuditLog lg
	join dbo.Game gm
		on Metadata like '%GameId:' + cast(gm.GameId as varchar) + '%'
where gm.MatchId = @matchId
end
go

exec sp_addrolemember 'db_datareader', 'TicTacUser'
exec sp_addrolemember 'db_datawriter', 'TicTacUser'

grant execute on dbo.sp_GetAIGamesForEvaluation to TicTacUser
go

grant execute on dbo.sp_GetAllLogsForMatch to TicTacUser
go