/* Clear initial data (if you so desire) */

DELETE FROM Users

DECLARE 
	@saltKey nvarchar(25) = '?',
	@date datetime = GETDATE()


/* Only Add initial data once */
IF (SELECT COUNT(*) FROM Users WHERE userId=1) = 0 BEGIN
	/* Create Default User (admin) account */
	INSERT INTO Users 
	(userId, email, salt, datecreated, enabled, usertype)
	VALUES 
	(1, 'admin@localhost', CONVERT(VARCHAR(32), HashBytes('MD5', 'admin@localhost'+@saltKey+'development'), 2), @date, 1, 0)
END

/* Add Empty Feed to represent articles that don't belong to a feed */
IF (SELECT COUNT(*) FROM Feeds WHERE feedId=0) = 0 BEGIN
	/* Create Default User (admin) account */
	INSERT INTO Feeds (feedId, title, url, lastChecked, filter)
	VALUES (0, 'No Feed', '', GETDATE(), '')
END