var Search;
var GetLine;
var GetSearch;
var RedirectTo;
var GetTopicDetail;
var SessionUser;
$(document).ready(function () {
    $('#ChangeRequestTable').DataTable( {
        "paging": true,
        "lengthChange": true,
        "searching": true,
        "ordering": true,
        "info": true,
        "autoWidth": true,
        "pageLength": 10,
    });

    var table_cr;
    table_cr = $('#ChangeRequestTable').DataTable();

    $("#Production").change(function () {
        var temp = $("#Production").val();
        console.log(temp);
        $("#Line").empty();
        $.ajax({
            type: "POST",
            url: GetLine,
            data: JSON.stringify({
                Production: temp,
            }),
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                $.each(response, function (i, item) {
                    $("#Line").append(new Option(item.line));
                });
          
            },
            error: function () {
                swal("Error", "Cannot Not Connect Database", "error");
            }
        });
    });
    $("#search").click(function () {

        //status
        var status;
        // if ($("#ST_pending").prop("checked")) {
        //     status = 7;
        // } else if ($("#ST_issued").prop("checked")) {
        //     status = 1;
        // } else if ($("#ST_check").prop("checked")) {
        //     status = 1;
        // } else if ($("#ST_approved").prop("checked")) {
        //     status = 1;
        // } else if ($("#ST_finished").prop("checked")) {
        //     status = 1;
        // }
        
        if ($("#ST_request").prop("checked")) {
            status = 7;
        } else if ($("#ST_review").prop("checked")) {
            status = 8;
        } else if ($("#ST_trial").prop("checked")) {
            status = 9;
        } else if ($("#ST_closed").prop("checked")) {
            status = 6;
        } 

        //over status
        var overstatus;
        if ($("#ST_review_complete").prop("checked")) {
            overstatus = 1;
        } else if ($("#ST_Confirm_complete").prop("checked")) {
            overstatus = 1;
        } else if ($("#ST_allstatus").prop("checked")) {
            overstatus = 1;
        } 
        //changeitem
        var Chosechangeitem;
        if ($("#changeitem_All").prop("checked")) {
            Chosechangeitem = 1;
        } else if ($("#changeitem_Yes").prop("checked")) {
            Chosechangeitem = 1;
        } else if ($("#changeitem_No").prop("checked")) {
            Chosechangeitem = 1;
        } 
     
        var temp_data = {
            'Type': $("#changeType").val(),
            'Status': status,
            'ProductType': $("#productType").val(),
            'Overstatus': overstatus,
            'Changeitem': $("#changeitem").val(),
            'ControlNo': $("#TNScontrolNo").val(),
            'Model': $("#Model").val(),
            'Chosechangeitem': Chosechangeitem,
            'Partno': $("#partno").val(),
            'Partname': $("#partname").val(),
            'Related': $("#related").val(),
            'Processname': $("#processname").val(),
            'Production': $("#Production").val(),
            'Line': $("#Line").val(),
        };
        
        $.ajax({
            type: "POST",
            url: GetSearch,
            data: JSON.stringify(temp_data),
            contentType: 'application/json; charset=utf-8',
            beforeSend: function(){
                $("#loading").removeClass('hidden');
            },
            complete: function(){
                $("#loading").addClass('hidden');
            },
            success: function (response) {
                console.log(response);
                table_cr.clear();
                table_cr.destroy();
                table_cr = $('#ChangeRequestTable').DataTable( {
                    data:response,
                    "order": [],
                    columnDefs: [{
                        "targets": 7,
                        "render": function (data, type, row) {
                            return `<span class="badge badge-warning">${data}</span>`;
                        }
                    },{
                        "targets": 8,
                        "render": function (data, type, row) {
                            if(response === null){
                                return null;
                            }else if(row.User_insert === SessionUser){
                            // }else{
                                return `<div class="btn-group-vertical"><button type="button" name="detail" id="${row.ID_Topic}" class="btn btn-info btn-sm mb-1" data-toggle="modal" data-target="#largeModal" onclick="RedirectToDetail(this)">`+
                                `<i class="fas fa-external-link-alt"></i></button>` +
                                `<button type="button" name="edit" id="${data}" class="btn btn-success btn-sm  mb-1" data-toggle="modal" data-target="#largeModal" onclick="EditTopic(this)">`+
                                `<i class="fas fa-pen"></i>`+
                                `</button></div>`;
                            }else{
                                return `<div class="btn-group-vertical"><button type="button" name="detail" id="${row.ID_Topic}" class="btn btn-info btn-sm  mb-1" data-toggle="modal" data-target="#largeModal">`+
                                `<i class="fas fa-external-link-alt"></i></button></div>`;
                            }
                        }
                    }],
                    columns: [
                        { data: 'Topic_type' },
                        { data: 'ID_Topic' },
                        { data: 'Change_item' },
                        { data: 'Product_type' },
                        { data: 'Model' },
                        { data: 'Revision' },
                        { data: 'PartNo' },
                        { data: 'PartName' },
                        { data: 'ID_Topic' },
                    ],
                });
            

                // $('#ChangeRequestTable').DataTable().fnClearTable();

                // $('#ChangeRequestTable').DataTable().fnAddData(response);

                // $('#loading').removeClass('hidden');
                // $("#submit").click();
            },
            error: function () {
            }
        });
    });

});

function RedirectToDetail(e) {
    var id = $(e).attr("id");
    window.open(`/Detail/Index/?id=${id}`);
}

function EditTopic(e) {
    var id = $(e).attr("id");
    window.open(`/Request/Index/?id=${id}`);
}