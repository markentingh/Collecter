﻿CREATE PROCEDURE [dbo].[UpdateDownload]
	@qid int = 0,
	@status int = 0
AS
	UPDATE DownloadQueue SET status=@status WHERE qid=@qid