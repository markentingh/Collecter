S.feeds = {
    load: function () {
        $('#btnaddfeed').off().on('click', S.feeds.buttons.showAddFeed);
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
    }
}

S.feeds.load();