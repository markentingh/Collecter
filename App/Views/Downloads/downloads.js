S.downloads = {
    hub: null,

    init: function () {
        $('.downloads .download > .button').on('click', S.downloads.start);

        //set up signalR hub
        S.downloads.hub = new signalR.HubConnectionBuilder().withUrl('/downloadhub').build();
        S.downloads.hub.on('update', S.downloads.update);
        S.downloads.hub.on('checked', S.downloads.checked);
        S.downloads.hub.start().catch(S.downloads.error);
    },

    start: function () {
        $('.downloads .download').hide();
        S.downloads.check();
        S.downloads.checkFeeds();
    },

    check: function () {
        S.downloads.hub.invoke('CheckQueue');
    },

    update: function (msg) {
        //receive command from SignalR
        var box = $('.downloads .accordion > .box')[0];
        var div = document.createElement('div');
        div.className = 'cli-line';
        div.innerHTML = '> ' + msg;
        $('.downloads .console').append(div);
        box.scrollTop = box.scrollHeight;
    },

    checked: function () {
        //check again in 10 seconds
        S.downloads.update('...');
        setTimeout(S.downloads.check, 10 * 1000);
    },

    checkFeeds: function () {
        S.downloads.hub.invoke('CheckFeeds');
        setTimeout(S.downloads.checkFeeds, 60 * 60 * 1000);
    },

    error: function (err) {
        S.downloads.update('An error occurred when communicating with the SignalR hub');
        console.log(err);
    }

};

S.downloads.init();