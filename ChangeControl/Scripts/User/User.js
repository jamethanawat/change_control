var mock_data = [
    {
        code:'63014',
        name:'Pakawat Smutkun',
        email:'pakawat.smutkun@email.thns.co.th',
        department:'null',
        active:'1',
        subscribe:'1',
        children: [
            {department: 'QC1'},
            {department: 'QC2'},
            {department: 'QC3'},
        ]
    }
];

$(document).ready(function () {
    $('#user_tb').DataTable( { 
        "paging": true,
        "lengthChange": true,
        "searching": true,
        "ordering": true,
        "info": true,
        "autoWidth": true,
        "pageLength": 10,
    });

    var table_cr;
    table_cr = $('#user_tb').DataTable();

    
    table_cr.clear();
                table_cr.destroy();
                table_cr = $('#user_tb').DataTable( {
                    data:[{
                        code:'63014',
                        name:'Pakawat Smutkun',
                        email:'pakawat.smutkun@email.thns.co.th',
                        department:'null',
                        active:'1',
                        subscribe:'1',
                        children: [
                            {department: 'QC1'},
                            {department: 'QC2'},
                            {department: 'QC3'},
                        ]
                    }],
                    treeGrid: {
                        left: 10,
                        expandIcon: '<span>+</span>',
                        collapseIcon: '<span>-</span>'
                    },
                    "order": [],
                    columns: [
                        // {
                        //     data: function (item) {
                        //         if (item.children != null && item.children.length > 0) {
                        //             return '<span> + </span>';
                        //         }
                        //         return '';
                        //     }
                        // },
                        {
                            title: '',
                            target: 0,
                            className: 'treegrid-control',
                            data: function (item) {
                                if (item.children) {
                                    return '<span>+</span>';
                                }
                                return '';
                            }
                        },
                        { data: 'code' },
                        { data: 'name' },
                        { data: 'email' },
                        { data: 'department' },
                        { data: 'active' },
                        { data: 'subscribe' },
                    ],
                    
                });
});



