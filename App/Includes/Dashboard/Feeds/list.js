S.feeds = {
    load: function () {
        $('#btnaddfeed').off().on('click', S.feeds.buttons.showAddFeed);
        $('#btnsaveaddfeed').off().on('click', S.feeds.buttons.saveAddFeed);
        $('#btncanceladdfeed').off().on('click', S.feeds.buttons.hideAddFeed);
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
        }
    }
}

S.feeds.load();