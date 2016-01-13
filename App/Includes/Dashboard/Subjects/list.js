S.subjects = {
    load: function () {
        $('#btnsaveselectedwords').off().on('click', S.subjects.buttons.saveSelectedWords);
        $('#btncancelselectedwords').off().on('click', S.subjects.buttons.hideSelectedWords);
        $('#btnaddsubjects').off().on('click', S.subjects.buttons.showSelectedWords);
    },

    buttons: {
        showSelectedWords(){
            $('.form-addsubjects').show();
            $('.btn-addsubjects').hide();
        },

        hideSelectedWords() {
            $('.form-addsubjects').hide();
            $('.btn-addsubjects').show();
        },

        expandSubject(id, pid) {
            var subj = $('#subjects' + pid);
            var subjbtn = $('#subject' + id);
            var boxlist = subj.find('.box-list');
            var boxPos = { h: S.elem.height(boxlist[0]) };
            boxlist.css({ 'max-height': boxPos.h });
            boxlist.animate({ 'max-height': 0 }, 333);
            subj.find('.selection').animate({ 'max-height': 50 }, 333);
            subj.animate({ 'padding-top': 0, 'padding-bottom': 0 }, 333);
            subj.find(".selection > .label")[0].innerHTML = subjbtn.find(">a")[0].innerHTML;
            setTimeout(function () { subj.addClass('selected'); }, 1000);
            subj.find('> .title').addClass('hide');
            S.ajax.post('/api/Dashboard/Subjects/GetSubjectsUI', { parentId:id }, function (data) {
                $('.subjects-list').append(data.d);
                S.subjects.load();
            });
        },

        saveSelectedWords() {
            var type = $('#lstwordtype').val();
            var words = $('#txtselectedwords').val();
            var grammartype = $('#lstwordpart').val();
            var hierarchy = $('#txtwordcategory').val();
            var score = $('#txtwordscore').val();

            switch (type) {
                case "subject":
                    S.ajax.post('/api/Dashboard/Subjects/AddSubject', { words: words, grammartype: grammartype, hierarchy: hierarchy, score: score }, function () { alert('subject(s) added'); });
                    break;
                case "common":
                    S.ajax.post('/api/Dashboard/Articles/AddCommonWord', { word: words }, function () { alert('common word(s) added'); });
                    break;
                case "phrase":
                    S.ajax.post('/api/Dashboard/Articles/AddPhrase', { word: words }, function () { alert('phrase added'); });
                    break;
                case "word": S.ajax.post('/api/Dashboard/Articles/AddWords', { words: words, grammartype: grammartype, hierarchy: hierarchy, score: score }, function () { alert('word(s) added'); });
                    break;
            }
        }
    }
}
S.subjects.load();