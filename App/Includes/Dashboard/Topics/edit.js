S.topics.edit = {
    edited: false,

    buttons: {
        editTopic: function (className) {
            $('.accordion.' + className + ' .preview').hide();
            $('.accordion.' + className + ' .edit').show();
            autosize.update($('#' + className + '-content'));
        },

        previewTopic: function (className) {
            var title = $('#' + className + '-title').val();
            var content = $('#' + className + '-content').val();
            var htm = marked(content
                    .replace(/\n{3,}/g, '\n\n!b!\n\n')
                    .replace(/\n{3,}/g, '\n\n!b!\n\n')
                ).replace(/\<p\>\!b\!\<\/p\>/g, '<br/>');
            $('.accordion.' + className + ' .edit, .preview .nopreview').hide();
            $('.accordion.' + className + ' .preview, .preview .ispreview').show();
            $('.accordion.' + className + ' .section-contents')[0].innerHTML = htm;
            $('.accordion.' + className + ' > .title').html(title);
        },

        saveChanges: function (groupName) {
            if(S.topics.edit.edited == false){return;}
            var i = 1;
            var sections = new Array();
            var obj;
            $('.btn-savechanges').hide();
            S.topics.edit.edited = false;
            while (i > 0) {
                obj = $('#' + groupName + i + '-title');
                if (obj.length == 1) {
                    sections.push({
                        title: obj.val(),
                        content: $('#' + groupName + i + '-content').val()
                    });
                    i++;
                } else { break; }
            }
            S.ajax.post('/api/Topics/SaveChanges', { sections: JSON.stringify(sections) }, function () { });
        }
    },

    texteditor: {
        keyDown: function (e) {
            if (S.topics.edit.edited == false) {
                $('.li-savechanges, .btn-savechanges').show();
                S.topics.edit.edited = true;
                autosize.update($(e));
            }
        },

        autoSize: function () {
            var ta = document.querySelector('textarea');
            
            //remove any existing autosizes
            var evt = document.createEvent('Event');
            evt.initEvent('autosize:destroy', true, false);
            ta.dispatchEvent(evt);

            //add new autosizes to all textareas on the page
            autosize($('textarea'));
        }
    },

    sections: {
        addAt: function (index) {

        },

        moveTo: function (index, above) {

        }
    }
};