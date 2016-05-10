S.topics = {
    list: [], urlCount: 0, cancelcheck: false, checkingfeeds: false,
    countdown: 0, timer: null, timerStart: null, timerEnd: null, timerCurrent: null,
    newTopic: {
        subjectId: 0,
        element: ''
    },

    load: function () {
        $('#btnaddtopic').off().on('click', S.topics.buttons.showAddTopic);
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
            var breadcrumb = $('#txtaddtopicsubject').val();
            var search = '';
            var sort = 0;
            S.ajax.post('/api/Topics/AddTopic', { breadcrumb: breadcrumb, title: title, description: description, search: search, sort: sort }, function (data) { $('.topics .contents').html(data.d); });
            $('#txtaddtopictitle').val('');
            $('#txtaddtopicdesc').val('');
            S.topics.buttons.hideAddTopic();
        }
    }
}

S.topics.load();