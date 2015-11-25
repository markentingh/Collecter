CREATE PROCEDURE [dbo].[AuthenticateUser]
	@email nvarchar(50) = '',
	@salt nvarchar(50) = ''
AS
	
SELECT u.userId, u.usertype, u.datecreated 
FROM Users u
WHERE email=@email AND salt=@salt AND enabled=1