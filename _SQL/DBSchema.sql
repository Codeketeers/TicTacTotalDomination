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
	NumberOfGames int not null,
	CreateDate datetime2(7) not null,
	WonDate datetime2(7) null,
	PlayerOneId int foreign key references dbo.Player not null,
	PlayerTwoId int foreign key references dbo.Player not null,
	WinningPlayerId int foreign key references dbo.Player null,
	constraint CK_Match_TwoPlayers check(PlayerOneId <> PlayerTwoId),
	constraint CK_Match_WinnerPlaying check(WinningPlayerId is null
										or WinningPlayerId = PlayerOneId
										or WinningPlayerId = PlayerTwoId),
)
go

create table dbo.Game(
	GameId int identity(1,1) primary key not null,
	MatchId int foreign key references dbo.Match null,
	PlayerOneId int foreign key references dbo.Player not null,
	PlayerTwoId int foreign key references dbo.Player not null,
	WinningPlayerId int foreign key references dbo.Player null,
	CurrentPlayerId int foreign key references dbo.Player null,
	CreateDate datetime2(7) not null,
	WonDate datetime2(7) null,
	constraint CK_Game_TwoPlayers check(PlayerOneId <> PlayerTwoId),
	constraint CK_Game_WinnerPlaying check(WinningPlayerId is null
										or WinningPlayerId = PlayerOneId
										or WinningPlayerId = PlayerTwoId),
	constraint CK_Game_CurrentPlayerPlaying check(CurrentPlayerId is null
												or CurrentPlayerId = PlayerOneId
												or CurrentPlayerId = PlayerTwoId)
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
	exec sp_addrolemember 'db_datareader', 'TicTacUser'
	exec sp_addrolemember 'db_datawriter', 'TicTacUser'
end