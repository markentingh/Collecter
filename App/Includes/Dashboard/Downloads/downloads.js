S.downloader = {
    list: [], totalDownloads: 0, downloading: false, countdown: 0, queueCheckMins: 2,
    timer: null, timerStart: null, timerEnd: null, timerCurrent: null, timerQueue: null,

    load: function () {
        $('#btnaddserver').off().on('click', S.downloader.buttons.showAddServer);
        $('#btndownload').off().on('click', S.downloader.buttons.startDownloads);
        $('#btnstopdownloader').off().on('click', S.downloader.buttons.cancelDownloadArticles);
        $('#btnsaveaddserver').off().on('click', S.downloader.buttons.saveAddServer);
        $('#btncanceladdserver').off().on('click', S.downloader.buttons.hideAddServer);
        $('#lstservertype').off().on('change', S.downloader.buttons.changeServerType);
    },

    buttons: {
        showAddServer: function () {
            $('.form-addserver').show();
            $('#btnaddserver').hide();
            S.downloader.buttons.changeServerType();
        },

        hideAddServer: function () {
            $('.form-addserver').hide();
            $('#btnaddserver').show();
        },

        saveAddServer: function () {
            var serverType = $('#lstservertype').val();
            var options = {};
            switch (serverType) {
                case '0': //local
                    options = { type: serverType, title: '', settings: $('#txtaddlocaladdress').val() };
                    break;
                case '1': //web server
                    var title = $('#txtaddservertitle').val();
                    var settings = $('#txtaddserveraddress').val();
                    options = { type: serverType, title: title, settings: settings };
                    break;
            }
            S.ajax.post('/api/Downloads/AddServer', options, function () { S.ajax.callback.inject(arguments[0]); });
            $('#txtaddservertitle').val('');
            $('#txtaddserveraddress').val('');
            S.downloader.buttons.hideAddServer();
        },

        cancelDownloadArticles() {
            S.downloader.downloading = false;
            clearTimeout(S.downloader.timer);
            $('.form-downloader, .downloader-status').hide();
            $('#btndownloader').show();
            $('.downloading').html('');
            S.downloader.stopQueueChecker();
        },

        startDownloads: function () {
            $('.form-downloader').show();
            $('#btndownloader').hide();
            S.downloader.downloading = true;
            $('.form-downloader .checking-msg')[0].innerHTML = "Distributing URLs to servers for downloading...";
            S.ajax.post('/api/Downloads/StartDownloads', {}, function () { S.ajax.callback.inject(arguments[0]); });
            S.downloader.startQueueChecker();
        },

        changeServerType: function () {
            var type = $('#lstservertype').val();
            $('.option-local, .option-webserver, .option-azure, .option-vpn').hide();
            $('').hide();
            switch (type) {
                case '0':
                    $('.option-local').show();
                    break;
                case '1':
                    $('.option-webserver').show();
                    break;
                case '2':
                    $('.option-azure').show();
                    break;
                case '3':
                    $('.option-vpn').show();
                    break
            }
        }
    },

    loadServerFrames: function (index, serverId, host) {
        var container = $('.server' + index + ' .downloading');
        var url = 'http://' + host + '/Download';
        container[0].innerHTML = '<iframe frameborder="0" scrolling="no" src="' + url + '"></iframe>';
    },

    updateDownloadQueue: function (minus, msg, classes) {
        if (S.downloader.downloading == false) { return false;}
        if (minus != 0) { S.downloader.totalDownloads -= minus; }
        if (msg != null) { S.downloader.consoleLog(msg, classes); }
        $('.servers .total')[0].innerHTML = S.util.math.numberWithCommas(S.downloader.totalDownloads);
        return true;
    },

    finishDownloads: function () {
        $('.downloading').html('');
        $('.form-downloader .checking-msg')[0].innerHTML = "Finished downloading all articles in the queue.";
        S.downloader.consoleLog('Finished downloading all articles in the queue. Waiting for new articles to download...', 'finished');
        S.downloader.downloading = false;
    },

    startQueueChecker: function () {
        clearTimeout(S.downloader.timerQueue);
        S.downloader.timerQueue = setTimeout(function () { S.downloader.checkQueue();}, 60000 * S.downloader.queueCheckMins)
    },

    checkQueue: function (){
        S.ajax.post('/api/Downloads/CheckQueue', {},
            function (data) {
                S.downloader.totalDownloads += data.d;
                if (data.d > 0) {
                    S.downloader.consoleLog('Added <b>' + data.d + '</b> URL(s) to the download queue.', 'added-queue');
                }
                S.downloader.updateDownloadQueue(0);
                if (S.downloader.downloading == false) {
                    S.downloader.buttons.startDownloads();
                }
        });
        S.downloader.startQueueChecker();
    },

    consoleLog: function (msg, classes) {
        if ($('.console .contents > div').length > 200) {
            $('.console .contents > div').remove();
        }
        var time = new Date();
        var hrs = time.getHours() % 12;
        var mins = time.getMinutes();
        var secs = time.getSeconds();
        hrs = (hrs ? hrs : 12);
        mins = (mins < 10 ? '0' + mins : mins);
        secs = (secs < 10 ? '0' + secs : secs);
        $('.console .contents').append('<div><span class="time">' + (hrs + ':' + mins + ':' + secs) + '</span><div class="msg ' + classes + '">' + msg + '</div></div>');
    },

    stopQueueChecker: function () {
        clearTimeout(S.downloader.timerQueue);
    }
}

S.downloader.load();