CREATE TABLE [dbo].[Users]
(
	[userId] INT NOT NULL PRIMARY KEY, 
    [email] NVARCHAR(50) NULL, 
    [salt] NVARCHAR(50) NULL, 
    [datecreated] DATETIME NULL, 
    [enabled] BIT NULL, 
    [usertype] TINYINT NULL
)
