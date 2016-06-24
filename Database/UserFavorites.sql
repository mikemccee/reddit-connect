CREATE TABLE [UserFavorites]
(
	[id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
	[userid] [int] NOT NULL,
	[contentid] [nvarchar](128) NOT NULL,
	[tagname] [nvarchar](256) NULL,
)
