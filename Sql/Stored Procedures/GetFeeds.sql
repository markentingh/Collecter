﻿CREATE PROCEDURE [dbo].[GetFeeds]
AS
SELECT * FROM Feeds WHERE feedId > 0 ORDER BY title ASC
