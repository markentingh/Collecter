/* Clear initial data (if you so desire) */

DECLARE @date datetime = GETDATE()

/* Setup Initial Variables */
IF (SELECT COUNT(*) FROM VarDates) = 0 BEGIN
	INSERT INTO VarDates (id, name, value) VALUES (1, 'download queue', GETDATE())
END

/* Add Empty Feed to represent articles that don't belong to a feed */
IF (SELECT COUNT(*) FROM Feeds WHERE feedId=0) = 0 BEGIN
	/* Create Default User (admin) account */
	INSERT INTO Feeds (feedId, title, url, lastChecked, filter)
	VALUES (0, 'No Feed', '', GETDATE(), '')
END