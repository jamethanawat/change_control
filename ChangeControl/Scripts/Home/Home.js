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
                console.log(response);
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
                            let action_btn = `<div class="btn-group"><button type="button" name="detail" id="${row.Code}" class="btn btn-info btn-sm  mb-1" data-toggle="modal" data-target="#largeModal" onclick="RedirectToDetail(this)" style="margin-right: 4px;">`+
                            `<i class="fas fa-external-link-alt"></i></button>`;
                            console.log(row);
                            console.log(SessionUser);
                            if(response === null){
                                return null;
                            }else if(row.User_insert === SessionUser){
                                action_btn += `<button type="button" name="edit" id="${row.Code}" class="btn btn-success btn-sm  mb-1" data-toggle="modal" data-target="#largeModal" onclick="EditTopic(this)">`+
                                `<i class="fas fa-pen"></i>`+
                                `</button>`;
                            }
                            action_btn += "</div>";
                            return action_btn;
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
                    scrollTop: $(document).height()
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