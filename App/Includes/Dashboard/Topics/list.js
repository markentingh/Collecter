S.topics = {
    list: [], urlCount: 0, cancelcheck: false, checkingfeeds: false,
    countdown: 0, timer: null, timerStart: null, timerEnd: null, timerCurrent: null,
    newTopic:{
        subjectId: 0,
        element: ''
    },

    load: function () {
        $('#btnsaveaddtopic').off().on('click', S.topics.buttons.saveAddTopic);
        $('#btncanceladdtopic').off().on('click', S.topics.buttons.hideAddTopic);
    },

    buttons: {
        showAddTopic: function (element, subjectId) {
            $('.form-addtopic').show();
            $('#btnaddtopic').hide();
        },

        hideAddTopic: function () {
            $('.form-addtopic').hide();
            $('#btnaddtopic').show();
        },

        saveAddTopic: function () {
            var title = $('#txtaddtopictitle').val();
            var description = $('#txtaddtopicdesc').val();
            S.ajax.post('/api/Topics/AddTopic', { element: S.topics.newTopic.element, subjectId: S.topics.newTopic.subjectId, title: title, description: description }, function () { S.ajax.callback.inject(arguments[0]); });
            $('#txtaddtopictitle').val('');
            $('#txtaddtopicdesc').val('');
            S.topics.buttons.hideAddTopic();
        }
    }
}

S.topics.load();