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
            var data = {
                title: $('#txtaddtopictitle').val(),
                description: $('#txtaddtopicdesc').val(),
                breadcrumb: $('#txtaddtopicsubject').val(),
                location: "",
                media: "",
                search: '',
                sort: 0
            };
            S.ajax.post('/Topics/CreateTopicFromBreadcrumb', data, function (d) {
                if (d.indexOf('success') == 0) {
                    location.href = 'topic/' + d.split('|')[1];
                } else {
                    //S.message.show()
                }
                
            });
            $('#txtaddtopictitle').val('');
            $('#txtaddtopicdesc').val('');
            S.topics.buttons.hideAddTopic();
        }
    }
}

S.topics.load();