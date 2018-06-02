S.subjects = {
    load: function () {
        $('#form-addsubjects').on('submit', function (e) {
            S.subjects.buttons.saveSubjects(); e.preventDefault(); return false;
        });
        $('#btnaddsubjects').off().on('click', S.subjects.add.show);
        $('#btnsavesubjects').off().on('click', S.subjects.add.save);
        $('#btncanceladdsubject').off().on('click', S.subjects.add.hide);
        $('#btnmovesubject').off().on('click', S.subjects.move.submit);
        $('#btncancelmovesubject').off().on('click', S.subjects.move.hide);
    },

    add: {
        show: function () {
            $('.form-addsubjects').show();
            $('#btnaddsubjects').hide();
        },

        showFromSubject: function (breadcrumb) {
            $('#txthierarchy').val(breadcrumb);
            $('#txtsubjects').val('');
            S.subjects.add.show();
        },

        hide: function () {
            $('.form-addsubjects').hide();
            $('#btnaddsubjects').show();
        },

        save: function () {
            var subjects = $('#txtsubjects').val();
            var hierarchy = $('#txthierarchy').val();

            S.ajax.post('/Subjects/AddSubjects', { subjects: subjects, hierarchy: hierarchy, loadUI: true },
                function (data) {
                    $('.subjects-list .accordion.subjects:not(.selected)').remove();
                    var d = JSON.parse(data);
                    d.d.inject = 1; //append
                    console.log(d);
                    S.ajax.inject('.subjects-list > .inner', d);
                });

            $('#txtsubjects').val('');
            $('#txthierarchy').val('');
            S.subjects.add.hide();
        }
    },

    select: {
        show: function (id, pid, breadcrumb, speed, noload) {
            var subj = $('#subjects' + pid);
            var subjlist = $('.subjects-list');
            var inner = $('.subjects-list > .inner');
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

            //set height of subjects list
            subjlist.css({ 'max-height': subjlist.height() });

            //animate height of subjects list
            subjlist.animate({ 'max-height': inner.height() - (boxPos.h - 50) }, speed);

            //hide options box
            box.hide();

            //resize parent subject section and hide sub-list
            boxlist.css({ 'max-height': boxPos.h });
            boxlist.animate({ 'max-height': 0 }, speed);
            subj.animate({ 'padding-bottom': 0 }, speed);
            subj.find('.selection').animate({ 'max-height': 50 }, speed);

            //set title of selected subject within parent subject section
            subj.find('.title').removeClass('hide').css({ 'max-height': 40, 'padding': '11px 15px', 'height': '17px' });
            subj.find(".selection > .label").html(title);
            subj.find('.title').animate({ 'max-height': 0, 'padding': '0 15px', 'height': 0 }, speed, function () {
                $(this).addClass('hide');
            });

            //set parent subject section as currently selected subject
            setTimeout(function () { subj.addClass('selected'); }, speed);

            //set events for selected subject options
            console.log(title);
            subj.find('.add-from-subject').off().attr('onclick', 'S.subjects.add.showFromSubject("' + breadcrumb + '"); return false;');
            subj.find('.move-from-subject').off().attr('onclick', 'S.subjects.move.show("' + id + '", "' + title + '","' + pbread + '"); return false;');
            subj.find('.goback').off().attr('onclick', 'S.subjects.select.cancel(\'' + pid + '\'); return false;');

            //load subjects UI for select subject
            if (noload != true) {
                S.ajax.post('/Subjects/LoadSubjectsUI', { parentId: id, getHierarchy: false, isFirst: false }, function (data) {
                    $('.subjects-list > .inner').append(data.d.html);
                    $('.subjects-list').animate({ 'max-height': inner.height() });
                    S.subjects.load();
                }, function () {
                    console.log(arguments);
                }, true);
            }
            S.subjects.hideAll();
        },

        showFromSubject(breadcrumb) {
            $('#txthierarchy').val(breadcrumb);
            $('#txtsubjects').val('');
            S.subjects.hideAll();
            S.subjects.add.show();
        },

        cancel: function (id) {
            var speed = 333;
            var subj = $('#subjects' + id);
            var boxlist = subj.find('.box-list');
            subj.removeClass('selected')
            boxlist.animate({ 'max-height': 1000 }, speed);
            subj.find('.selection').animate({ 'max-height': 0 }, speed);
            subj.animate({ 'padding-top': 20, 'padding-bottom': 0 }, speed);
            subj.find('.title').removeClass('hide').animate({ 'max-height': 40, 'padding': '11px 15px', 'height': '17px' }, speed);
            subj.nextAll().remove();
        }
    },

    move: {
        show: function (id, name, breadcrumb) {
            $('#txtmovesubjectid').val(id);
            $('#txtmovetohierarchy').val(breadcrumb);
            $('.movesubjectname').html('"' + name + '"');
            S.subjects.hideAll();
            $('.form-movesubject').show();
            $('#btnaddsubjects').hide();
        },

        hide: function () {
            $('.form-movesubject').hide();
            $('#btnaddsubjects').show();
        },

        submit: function () {
            var id = $('#txtmovesubjectid').val();
            var hier = $('#txtmovetohierarchy').val();
            S.ajax.post('/Subjects/MoveSubject', { id: id, hierarchy: hier, element: '.subjects-list > .inner' },
                function (data) {
                    S.ajax.inject('', JSON.parse(data));
                }
            );
            S.subjects.move.hide();
        }
    },

    hideAll: function () {
        $('.form-addsubjects, .form-movesubject').hide();
    }
}
S.subjects.load();