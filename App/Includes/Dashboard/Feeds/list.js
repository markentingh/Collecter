S.feeds = {
    list: [], urlCount: 0, cancelcheck: false, checkingfeeds: false,
    countdown: 0, timer: null, timerStart: null, timerEnd: null, timerCurrent: null,

    load: function () {
        $('#btnaddfeed').off().on('click', S.feeds.buttons.showAddFeed);
        $('#btncheckfeeds').off().on('click', S.feeds.buttons.showFeedsCheck);
        $('#btncancelfeedcheck, #btnstopfeedcheck').off().on('click', S.feeds.buttons.cancelFeedsCheck);
        $('#btnstartfeedcheck').off().on('click', S.feeds.buttons.checkFeeds);
        $('#btnsaveaddfeed').off().on('click', S.feeds.buttons.saveAddFeed);
        $('#btncanceladdfeed').off().on('click', S.feeds.buttons.hideAddFeed);
        $('#lstfeedtype').off().on('change', S.feeds.buttons.changeFeedType);
    },

    buttons: {
        showAddFeed: function () {
            $('.form-addfeed').show();
            $('#btnaddfeed').hide();
        },

        hideAddFeed: function () {
            $('.form-addfeed').hide();
            $('#btnaddfeed').show();
        },

        saveAddFeed: function () {
            var title = $('#txtaddfeedtitle').val();
            var url = $('#txtaddfeedurl').val();
            S.ajax.post('/api/Feeds/AddFeed', { title: title, url: url }, function () { S.ajax.callback.inject(arguments[0]); });
            $('#txtaddfeedtitle').val('');
            $('#txtaddfeedurl').val('');
            S.feeds.buttons.hideAddFeed();
        },

        showFeedsCheck(){
            $('.form-feedscheck').show();
            $('#btncheckfeeds').hide();
        },

        cancelFeedsCheck() {
            S.feeds.checkingfeeds = false;
            S.feeds.cancelcheck = true;
            clearTimeout(S.feeds.timer);
            $('.form-feedscheck, .feeds-status').hide();
            $('.form-feedscheck .settings').show();
            $('.form-feedscheck .checking').hide();
            $('#btncheckfeeds').show();
        },

        checkFeeds: function () {
            S.feeds.checkingfeeds = true;
            S.feeds.urlCount = 0;
            $('.feeds-status .progress').css({ width: '0%' });
            $('.feeds-status').addClass('checking');
            $('.feeds-status .progress-msg')[0].innerHTML = "Checking available feeds for new links...";
            $('.form-feedscheck .settings').hide();
            $('.form-feedscheck .checking, .feeds-status').show();
            S.ajax.post('/api/Feeds/CheckFeeds', {}, function () { S.ajax.callback.inject(arguments[0]); });
            S.feeds.countdown = parseInt($('#txtcheckmins').val()); 
            S.feeds.timerStart = new Date();
            S.feeds.timerEnd = new Date(S.feeds.timerStart.getTime() + S.feeds.countdown * 60000);
            S.feeds.countdownFeeds();
        },

        checkFeed: function (index) {
            var more = false;
            if (S.feeds.cancelcheck == true) {
                S.feeds.cancelcheck = false;
                return;
            }
            if (S.feeds.checkingfeeds == true) {
                more = true;
            } else {
                $('.feeds-status .progress').css({ width: '0%' });
                $('.feeds-status').addClass('checking');
                $('.feeds-status .progress-msg')[0].innerHTML = "Checking selected feed for new links...";
                $('.feeds-status').show();
            }
            S.ajax.post('/api/Feeds/CheckFeed', { index: index, more: more }, function () { S.ajax.callback.inject(arguments[0]); });
        },

        changeFeedType: function () {
            var type = $('#lstfeedtype').val();
            $('.option-feedurl').hide();
            $('.option-feedupload').hide();
            switch (type) {
                case '1': case '2':
                    $('.option-feedurl').show();
                    break;
                case '3':
                    $('.option-feedupload').show();
                    break
            }
        }
    },

    updateFeedStatus: function (domain, total, index) {
        S.feeds.urlCount += total;
        $('.feeds-status .progress-msg')[0].innerHTML = "added " + total + " url(s) from " + domain + ".";
        $('.feeds-status .progress').css({width: Math.round((100 / S.feeds.list.length) * (index + 1)) + '%'});
    },

    checkedAllFeeds: function (){
        $('.feeds-status .progress-msg')[0].innerHTML = S.feeds.urlCount + " total url(s) collected from " + S.feeds.list.length + " feed(s).";
        $('.feeds-status .progress').css({ width: '100%' });
        S.feeds.checkingfeeds = false;
    },

    checkedFeed: function (domain) {
        $('.feeds-status .progress-msg')[0].innerHTML = S.feeds.urlCount + " total url(s) collected from " + domain + ".";
        $('.feeds-status .progress').css({ width: '100%' });
    },

    countdownFeeds: function () {
        var t = S.feeds.timerCurrent = S.feeds.timerEnd - new Date();
        var seconds = Math.floor((t / 1000) % 60);
        var minutes = Math.floor((t / 1000 / 60) % 60);
        var hours = Math.floor((t / (1000 * 60 * 60)) % 24);
        var days = Math.floor(t / (1000 * 60 * 60 * 24));
        if (seconds <= 0 && minutes <= 0 && hours <= 0 && days <= 0) {
            //time is up, check feeds again
            S.feeds.buttons.checkFeeds();
        } else {
            //continue counting down
            $('.form-feedscheck .checking-msg')[0].innerHTML = "Automated Feeds Check: Countdown til next check: " +
            (hours > 9 ? "" + hours : "0" + hours) + ":" +
            (minutes > 9 ? "" + minutes : "0" + minutes) + ":" +
            (seconds > 9 ? "" + seconds : "0" + seconds);
            S.feeds.timer = setTimeout(function () { S.feeds.countdownFeeds(); }, 1000);
        }

    },

    loadChart: function (feedId, data) {
        var paper = Raphael('paperfeed' + feedId, 100, 36);
        var setDots = paper.set();
        var setLines = paper.set();
        var lastxy = null;
        var x, y;
        for (var item in data) {
            //create dots & lines on chart
            x = parseInt(data[item][0]);
            y = parseInt(data[item][1]);
            //create a dot
            var dot = paper.circle(x, y, 2);
            setDots.push(dot);
            if (lastxy != null) {
                //create a line
                var line = paper.path('M' + lastxy[0] + ' ' + lastxy[1] + 'l' + (x - lastxy[0]) + ' ' + (y - lastxy[1]));
                setLines.push(line);
            }
            lastxy = [x, y];
        }
        setDots.attr({ 'fill': '#aaa', 'stroke-opacity':0 });
        setLines.attr({ 'stroke': '#aaa' });
    }
}

S.feeds.load();