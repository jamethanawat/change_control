﻿var mock_data = [
    {
        code:'63014',
        name:'Pakawat Smutkun',
        email:'pakawat.smutkun@email.thns.co.th',
        department:'Special',
        active:'',
        subscribe:'',
        children: [
            {
                code:'',
                name:'',
                email:'',
                department:'QC1',
                active:'1',
                subscribe:'0',
            },
            {
                code:'',
                name:'',
                email:'',
                department:'QC2',
                active:'1',
                subscribe:'1',
            },
            {
                code:'',
                name:'',
                email:'',
                department:'QC3',
                active:'0',
                subscribe:'0',
            },
        ]
    },
    // {
    //     code:'63014',
    //     name:'Pakawat Smutkun',
    //     email:'pakawat.smutkun@email.thns.co.th',
    //     department:'OT',
    //     active:'1',
    //     subscribe:'1',
    //     children: []
    // }
];
$(document).ready(function () {
    var dataTable = $('#user_tb').DataTable( {
        // "paging": true,
        // "lengthChange": true,
        // "searching": true,
        // "ordering": false,
        // "info": true,
        // "autoWidth": true,
        // "pageLength": 10,
        data:mock_data,
        treeGrid: {
            left: 10,
            expandIcon: '<span>+</span>',
            collapseIcon: '<span>-</span>'
        },
        columns: 
        [
            {
                target: 0,
                className: 'treegrid-control',
                data: function (item) {
                    if(item.children != null && item.children.length > 0){
                        return '<span>+</span>';
                    }
                    return '';
                }
            },
            { data: 'code', },
            { data: 'name' },
            { data: 'email' },
            { data: 'department',
              className: 'center'
            },
            { 
                target: 5,
                className: 'center', 
                data: function (item){
                    let checked = (item.active == 1) ? "checked" : "";
                    return (item.active != "")?`<input type="checkbox" class="js-switch" ${checked} />`:"";
                } 
            },
            { 
                target: 6,
                className: 'center', 
                data: function (item){
                    let checked = (item.subscribe == 1) ? "checked" : "";
                    return (item.subscribe != "")?`<input type="checkbox" class="js-switch" ${checked} />`:"";
                } 
            },
        ],
    });

    // $(".treegrid-control").click();

    // var elems = Array.prototype.slice.call(document.querySelectorAll('.js-switch'));
    // elems.forEach(function(html) { var switchery = new Switchery(html); });
});



