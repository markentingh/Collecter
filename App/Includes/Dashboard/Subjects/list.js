S.subjects = {
    load: function () {
        $('#btnsaveselectedwords').off().on('click', S.subjects.buttons.saveSelectedWords);
        $('#btncancelselectedwords').off().on('click', S.subjects.buttons.hideSelectedWords);
        $('#btnaddsubjects').off().on('click', S.subjects.buttons.showSelectedWords);
        $('#btnmovesubject').off().on('click', S.subjects.buttons.saveMoveSubject);
        $('#btncancelmovesubject').off().on('click', S.subjects.buttons.hideMoveSubject);
    },

    buttons: {
        hideAll: function(){
            $('.form-addsubjects, .form-movesubject').hide();
        },

        showSelectedWords: function (){
            $('.form-addsubjects').show();
            $('.btn-addsubjects').hide();
        },

        hideSelectedWords: function () {
            $('.form-addsubjects').hide();
            $('.btn-addsubjects').show();
        },

        showMoveSubject: function () {
            $('.form-movesubject').show();
            $('.btn-addsubjects').hide();
        },

        hideMoveSubject: function () {
            $('.form-movesubject').hide();
            $('.btn-addsubjects').show();
        },

        selectSubject: function (id, pid, breadcrumb, speed, noload) {
            var subj = $('#subjects' + pid);
            var subjbtn = $('#subject' + id);
            var title = subjbtn.find(">a")[0].innerHTML;
            var boxlist = subj.find('.box-list');
            var boxPos = { h: S.elem.height(boxlist[0]) };
            var pbread = '';
            console.log(breadcrumb); 
            if (breadcrumb.length > 0) {
                var bread = breadcrumb.split('>');
                console.log(bread);
                if (bread.length > 1) {
                    bread.splice(bread.length - 1, 1);
                    pbread = bread.join('>');
                    console.log(pbread);
                }
            }

            boxlist.css({ 'max-height': boxPos.h });
            boxlist.animate({ 'max-height': 0 }, speed);
            subj.find('.selection').animate({ 'max-height': 50 }, speed);
            subj.animate({ 'padding-top': 0, 'padding-bottom': 0 }, speed);
            subj.find(".selection > .label")[0].innerHTML = title;
            setTimeout(function () { subj.addClass('selected'); }, speed * 3);
            subj.find('> .title').addClass('hide');
            subj.find('.add-from-subject').off().attr('onclick', 'S.subjects.buttons.addFromSubject("' + breadcrumb + '"); return false;');
            subj.find('.move-from-subject').off().attr('onclick', 'S.subjects.buttons.moveFromSubject("' + id + '", "' + pbread + '"); return false;');
            subj.find('.calc-related-words').off().attr('onclick', 'S.subjects.buttons.showRelatedWordsForm("' + id + '","' + pid + '")');
            subj.find('.goback').off().attr('onclick', 'S.subjects.buttons.cancelSelectSubject(\'' + pid + '\'); return false;');
            if (noload != true) {
                S.ajax.post('/api/Subjects/LoadSubjectsUI', { parentId: id, arg2: false, arg3: false }, function (data) {
                    $('.subjects-list').append(data.d);
                    S.subjects.load();
                });
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
            S.subjects.buttons.showSelectedWords();
        },

        moveFromSubject: function (id, breadcrumb) {
            $('#txtmovesubjectid').val(id);
            $('#txtmovetohierarchy').val(breadcrumb);
            S.subjects.buttons.hideAll();
            S.subjects.buttons.showMoveSubject();
        },

        showRelatedWordsForm: function (id, parentid) {
            var subjbtn = $('#subject' + id);
            var title = subjbtn.find(">a")[0].innerHTML;
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
            S.ajax.post('/api/Subjects/LoadRelatedWordsUI', { id: id, pid:pid, search: $('#txtrelatedphrase' + id).val() }, function (data) {
                S.ajax.callback.inject(data);
                S.subjects.load();
            });
        },

        saveSelectedWords: function () {
            var type = $('#lstwordtype').val();
            var words = $('#txtselectedwords').val();
            var grammartype = $('#lstwordpart').val();
            var hierarchy = $('#txtwordcategory').val();
            var score = $('#txtwordscore').val();

            switch (type) {
                case "subject":
                    S.ajax.post('/api/Subjects/AddSubject', { words: words, grammartype: grammartype, hierarchy: hierarchy, score: score, loadUI: true, element: '.subjects-list' }, function () { S.ajax.callback.inject(arguments[0]); });
                    break;
                case "common":
                    S.ajax.post('/api/Articles/AddCommonWord', { word: words }, function () { alert('common word(s) added'); });
                    break;
                case "phrase":
                    S.ajax.post('/api/Articles/AddPhrase', { word: words }, function () { alert('phrase added'); });
                    break;
                case "word": S.ajax.post('/api/Articles/AddWords', { words: words, grammartype: grammartype, hierarchy: hierarchy, score: score }, function () { alert('word(s) added'); });
                    break;
            }
            $('#txtselectedwords').val('');
            S.subjects.buttons.hideSelectedWords();

        },

        saveMoveSubject: function () {
            var id = $('#txtmovesubjectid').val();
            var hier = $('#txtmovetohierarchy').val();
            S.ajax.post('/api/Subjects/MoveSubject', { id: id, hierarchy: hier, element: '.subjects-list' }, function () { S.ajax.callback.inject(arguments[0]); });
            S.subjects.buttons.hideMoveSubject();
        }


    }
}
S.subjects.load();