CREATE TABLE [Users]
(
	[id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
	[email] [nvarchar](128) NOT NULL,
	[pass] [ntext] NULL,
	[pass_salt] [ntext] NULL,
	[token] [uniqueidentifier] NULL,
)
