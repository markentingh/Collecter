CREATE TABLE [dbo].[Topics]
(
	[topicId] INT NOT NULL PRIMARY KEY, 
    [subjectId] INT NULL, 
    [geolat] FLOAT NULL, 
    [geolong] FLOAT NULL, 
    [datecreated] DATETIME NULL, 
    [title] NVARCHAR(250) NULL, 
	[location] NVARCHAR (250) NULL,
    [summary] NVARCHAR(MAX) NULL, 
    [media] NVARCHAR(MAX) NULL
)
