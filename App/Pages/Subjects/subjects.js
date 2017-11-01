S.subjects = {
    newTopic:{
        subjectId: 0,
        parentId: 0
    },
    load: function () {
        $('#form-addsubjects').on('submit', function (e) {
            S.subjects.buttons.saveSubjects(); e.preventDefault(); return false;
        });
        $('#btnaddsubjects').off().on('click', S.subjects.buttons.showAddSubject);
        $('#btnsavesubjects').off().on('click', S.subjects.buttons.saveSubjects);
        $('#btncanceladdsubject').off().on('click', S.subjects.buttons.hideAddSubject);
        $('#btnmovesubject').off().on('click', S.subjects.buttons.saveMoveSubject);
        $('#btncancelmovesubject').off().on('click', S.subjects.buttons.hideMoveSubject);
        $('#btnsaveaddtopic').off().on('click', S.subjects.buttons.addTopic);
        $('#btncanceladdtopic').off().on('click', S.subjects.buttons.hideAddTopic);
    },

    buttons: {
        hideAll: function(){
            $('.form-addsubjects, .form-movesubject').hide();
        },

        showAddSubject: function (){
            $('.form-addsubjects').show();
            $('#btnaddsubjects').hide();
        },

        hideAddSubject: function () {
            $('.form-addsubjects').hide();
            $('#btnaddsubjects').show();
        },

        saveSubjects: function () {
            var subjects = $('#txtsubjects').val();
            var hierarchy = $('#txthierarchy').val();

            S.ajax.post('/Subjects/AddSubjects', { subjects: subjects, hierarchy: hierarchy, loadUI:true },
                function (data) {
                    S.ajax.inject('.subjects-list', data);
                });

            $('#txtsubjects').val('');
            $('#txthierarchy').val('');
            S.subjects.buttons.hideAddSubject();
        },

        showMoveSubject: function () {
            $('.form-movesubject').show();
            $('#btnaddsubjects').hide();
        },

        hideMoveSubject: function () {
            $('.form-movesubject').hide();
            $('#btnaddsubjects').show();
        },

        selectSubject: function (id, pid, breadcrumb, speed, noload) {
            var subj = $('#subjects' + pid);
            var topic = $('#topicsfor' + pid);
            var subjbtn = $('#subject' + id);
            var title = subjbtn.find("a").html();
            var box = $('#subjects' + pid + ' .option-box');
            var boxlist = subj.find('.box-list');
            var boxPos = { h: $(boxlist[0]).height() };
            var pbread = '';
            if (breadcrumb.length > 0) {
                var bread = breadcrumb.split('>');
                if (bread.length > 1) {
                    bread.splice(bread.length - 1, 1);
                    pbread = bread.join('>');
                }
            }

            box.hide();
            boxlist.css({ 'max-height': boxPos.h });
            boxlist.animate({ 'max-height': 0 }, speed);
            subj.find('.selection').animate({ 'max-height': 50 }, speed);
            subj.animate({ 'padding-bottom': 0 }, speed);
            subj.find(".selection > .label").html(title);
            setTimeout(function () { subj.addClass('selected'); }, speed);
            subj.find('.title').addClass('hide');
            subj.find('.add-from-subject').off().attr('onclick', 'S.subjects.buttons.addFromSubject("' + breadcrumb + '"); return false;');
            subj.find('.move-from-subject').off().attr('onclick', 'S.subjects.buttons.moveFromSubject("' + id + '", "' + pbread + '"); return false;');
            subj.find('.calc-related-words').off().attr('onclick', 'S.subjects.buttons.showRelatedWordsForm("' + id + '","' + pid + '")');
            topic.find('.add-topic').attr('onclick', 'S.subjects.buttons.showAddTopic(' + id + ',' + pid + ',"'+breadcrumb+'")');
            subj.find('.topics').off().attr('onclick', 'S.subjects.buttons.viewTopics(' + id + ',' + pid + ')');
            topic.find('.topic-title').html('Topics for ' + breadcrumb.replace(/\>/g, ' &gt; '));
            subj.find('.goback').off().attr('onclick', 'S.subjects.buttons.cancelSelectSubject(\'' + pid + '\'); return false;');
            if (noload != true) {
                S.ajax.post('/Subjects/LoadSubjectsUI', { parentId: id, getHierarchy: false, isFirst: false }, function (data) {
                    $('.subjects-list').append(data.d.html);
                    S.subjects.load();
                }, function () {
                    console.log(arguments);
                }, true);
            }
            S.subjects.buttons.hideAll();
        },

        cancelSelectSubject: function (id) {
            var speed = 333;
            var subj = $('#subjects' + id);
            var boxlist = subj.find('.box-list');
            subj.removeClass('selected')
            boxlist.animate({ 'max-height': 1000 }, speed);
            subj.find('.selection').animate({ 'max-height': 0 }, speed);
            subj.animate({ 'padding-top': 20, 'padding-bottom': 0 }, speed);
            subj.nextAll().remove();
        },

        addFromSubject:function(breadcrumb){
            $('#txtwordcategory').val(breadcrumb);
            $('#lstwordtype').val('subject');
            $('#txtselectedwords').val('');
            S.subjects.buttons.hideAll();
            S.subjects.buttons.showAddSubject();
        },

        moveFromSubject: function (id, breadcrumb) {
            $('#txtmovesubjectid').val(id);
            $('#txtmovetohierarchy').val(breadcrumb);
            S.subjects.buttons.hideAll();
            S.subjects.buttons.showMoveSubject();
        },

        showRelatedWordsForm: function (id, parentid) {
            var subjbtn = $('#subject' + id);
            var title = subjbtn.find("a").html();
            var box = $('#subjects' + parentid + ' .option-box');
            box.html('<div class="row bottom">' +
                    '<div class="column"><h5>Get Highest Ranked Words</h5></div>' +
                '</div>' +
                '<div class="row bottom">' +
                    '<div class="column label">Search Phrase</div>' +
                    '<div class="column input"><input type="text" id="txtrelatedphrase' + id + '" style="width:250px;" value="' + title + '"/></div>' +
                    '<div class="column input"><div class="button green" onclick="S.subjects.buttons.getRelatedWords(' + id + ',' + parentid + ')">Search</div></div>' +
                    '<div class="column input"><div class="button">Cancel</div></div>' +
                '</div>');
            box.show();
        },

        getRelatedWords: function(id, pid){
            S.ajax.post('/Subjects/LoadRelatedWordsUI', { id: id, pid:pid, search: $('#txtrelatedphrase' + id).val() }, function (data) {
                S.ajax.callback.inject(data);
                S.subjects.load();
            });
        },

        saveMoveSubject: function () {
            var id = $('#txtmovesubjectid').val();
            var hier = $('#txtmovetohierarchy').val();
            S.ajax.post('/Subjects/MoveSubject', { id: id, hierarchy: hier, element: '.subjects-list' }, function () { S.ajax.callback.inject(arguments[0]); });
            S.subjects.buttons.hideMoveSubject();
        },

        viewTopics(subjectId, parentId) {
            $('#topicsfor' + parentId).show();
            S.ajax.post('/Subjects/GetTopics', { element: '#topicsfor' + parentId + ' .topics-list', subjectId: subjectId, start: 1, search: '', orderby: 1 }, function () { S.ajax.callback.inject(arguments[0]); });

        },

        showAddTopic: function (subjectId, parentId, breadcrumb) {
            S.subjects.newTopic.subjectId = subjectId;
            S.subjects.newTopic.parentId = parentId;
            $('#txtaddtopicsubject').val(breadcrumb.replace(/\>/g,' > '));
            $('.form-addtopic').show();
            $('#btnaddtopic').hide();
        },

        hideAddTopic: function () {
            S.subjects.newTopic.subjectId = 0;
            S.subjects.newTopic.parentId = 0;
            $('.form-addtopic').hide();
            $('#btnaddtopic').show();
            $('#txtaddtopictitle').val('');
            $('#txtaddtopicdesc').val('');
            $('#txtaddtopicsubject').val('');
        },

        addTopic: function () {
            var subjectId = S.subjects.newTopic.subjectId;
            var parentId = S.subjects.newTopic.parentId;
            var title = $('#txtaddtopictitle').val();
            var desc = $('#txtaddtopicdesc').val();
            S.ajax.post('/Subjects/AddTopic', { element: '#topicsfor' + parentId + ' .topics-list', subjectId: subjectId, title: title, description: desc }, function () { S.ajax.callback.inject(arguments[0]); });
            S.subjects.buttons.hideAddTopic();
        }
    }
}
S.subjects.load();