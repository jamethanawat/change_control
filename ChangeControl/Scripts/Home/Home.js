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
    $("#search").click((e) => {
        e.preventDefault();
        
        //changeitem
        var Chosechangeitem;
        if($("#changeitem_All").prop("checked")) {
            Chosechangeitem = 1;
        }else if($("#changeitem_Yes").prop("checked")) {
            Chosechangeitem = 1;
        }else if($("#changeitem_No").prop("checked")) {
            Chosechangeitem = 1;
        } 
     
        var temp_data = {
            'Type': $("#changeType").val(),
            'Status': $("input[name='status']:checked").val(),
            'ProductType': $("#productType").val(),
            'Overstatus': $("input[name='overstatus']:checked").val(),
            'Changeitem': $("#changeitem").val(),
            'ControlNo': $("#TNScontrolNo").val(),
            'Model': $("#Model").val(),
            'Chosechangeitem': Chosechangeitem,
            'Partno': $("#partno").val(),
            'Partname': $("#partname").val(),
            'Department': $("[name='dept']").val(),
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
                table_cr.clear();
                table_cr.destroy();
                table_cr = $('#ChangeRequestTable').DataTable( {
                    data:response,
                    "order": [],
                    columnDefs: [
                    {"targets": 3,
                        createdCell: (td) => { 
                            $(td).css('text-align', 'left'); 
                        }
                    },
                    {"targets": 6,
                        "render": function (data, type, row) {
                            return `${data}`;
                            // return `<span class="badge badge-warning">${data}</span>`;
                        }
                    },{"targets": 7,
                        "render": function (data, type, row) {
                            let action_btn = '';
                            if(response === null){
                                return null;
                            }else{
                                    var btn_badge = `secondary`;
                                    var editable = `disabled`;
                                    if(row.User_insert === SessionUser && row.FullStatus == "Request"){
                                        btn_badge = `success`;
                                        editable = `onclick="EditTopic(this)"`;
                                    }
                                    return `<div class="btn-group"><a href="Detail/?id=${row.Code}"><button type="button" name="detail" id="${row.Code}" class="btn btn-info btn-sm mb-1 mr-1" data-toggle="modal" data-target="#largeModal" >`+
                                    `<i class="fas fa-external-link-alt"></i></button></a><button type="button" name="edit" id="${row.Code}" class="btn btn-`+
                                    btn_badge + ` btn-sm  mb-1" data-toggle="modal" data-target="#largeModal"` +
                                    editable + `>`+
                                    `<i class="fas fa-pen"></i>`+
                                    `</button></div>`;
                            }
                        }
                    }],
                    columns: [
                        { data: 'Code' },
                        { data: 'Date' },
                        { data: 'Department' },
                        { data: 'Detail' },
                        { data: 'Product_type' },
                        { data: 'Model' },
                        { data: 'FullStatus' },
                    ],
                });

                $("body").addClass("sidebar-collapse")

                $("body, html").animate({
                    scrollTop: 700
                }, 100)
            },
            error: function () {
            }
        });
    });

});

function RedirectToDetail(e) {
    var id = $(e).attr("id");
    window.open(`Detail/?id=${id}`);
}

function EditTopic(e) {
    var id = $(e).attr("id");
    window.open(`Request/?id=${id}`);
}