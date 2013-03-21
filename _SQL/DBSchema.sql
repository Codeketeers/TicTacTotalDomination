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

create table dbo.Game(
	GameId int identity(1,1) primary key not null,
	PlayerOneId int foreign key references dbo.Player not null,
	PlayerTwoId int foreign key references dbo.Player not null,
	WinningPlayerId int foreign key references dbo.Player null,
	CreateDate datetime2(7) not null,
	WonDate datetime2(7) null,
	constraint CK_TwoPlayers check(PlayerOneId <> PlayerTwoId),
	constraint CK_WinnerPlaying check(WinningPlayerId is null
										or WinningPlayerId = PlayerOneId
										or WinningPlayerId = PlayerTwoId),
)
go

create table dbo.GameMove(
	MoveId int identity(1,1) primary key not null,
	GameId int foreign key references dbo.Game not null,
	PlayerId int foreign key references dbo.Player not null,
	MoveDate datetime2(7) not null,
	X int not null,
	y int not null,
)
go