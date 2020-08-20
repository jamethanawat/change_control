var Search;
var GetLine;
var GetSearch;
var GetTopicDetail;
var SessionUser;
$(document).ready(function () {
    $("body").addClass("sidebar-collapse", 1000);
    $('#ChangeRequestTable').DataTable( { 
        "paging": true,
        "lengthChange": true,
        "searching": true,
        "ordering": true,
        "info": true,
        "autoWidth": true,
        "pageLength": 10,
    });

/* ----------------------- Add pleaceholder to TNS No ----------------------- */

    $("#TNScontrolNo").mask('SS-0000000', {placeholder: "__-_______"});  
    $("#changeType").on("change",(e) => {
        $("#TNScontrolNo").val("");
        var placeholder = "__-_______";
        if(e.target.value == "Internal Change"){
            placeholder = "IN-_______";
        }else if(e.target.value == "External Change"){
            placeholder = "EX-_______";
        }else{
            placeholder = "__-_______";
        }
        $("#TNScontrolNo").attr("placeholder",placeholder);
    });

/* ------------------------------ Add pre-word ------------------------------ */

    $("#TNScontrolNo").on("click",(e) => {
        $("input[name='status']").prop("checked",false);
        let chang_type_val = $("#changeType").val();
        if(chang_type_val  == "Internal"){
            e.target.value = "IN-";
        }else if(chang_type_val  == "External"){
            e.target.value = "EX-";
        }
    });
    var table_cr;
    table_cr = $('#ChangeRequestTable').DataTable();

/* ----------------------- Overstatus relate to status ---------------------- */
    $("[name='overstatus']").change((e) => {
        if(e.target.value != 1) {
            let selected_st = 6 + Number(e.target.value);
            $("[name='status']").val([`${selected_st}`]);
        }
    })

/* ---------------------- Status related to overstatus ---------------------- */

    $("[name='status']").change((e) => {
            $("[name='overstatus']").val(['1']);
    })


/* ---------------------- Get line relate to production --------------------- */

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

/* -------------------------------------------------------------------------- */
/*                            Clear radio and input                           */
/* -------------------------------------------------------------------------- */

    $("#clear_btn").click(() => {
        $("#changeType").val("");
        $("#productType").val("");
        $("#changeitem").val("");
        $("#TNScontrolNo").val("");
        $("#Model").val("");
        $("[name = 'dept']").val("");
        $("#partno").val("");
        $("#partname").val("");
        $("[name='dept']").val("");
        $("#processname").val("");
        $("[name='overstatus'][value=1]").prop("checked",true)
        $("[name='status'][value=7]").prop("checked",true)
        $("#TNScontrolNo").val("");
        $("#TNScontrolNo").attr("placeholder","__-_______");
    });

/* -------------------------------------------------------------------------- */
/*                                Search topic                                */
/* -------------------------------------------------------------------------- */

    $("#search").click((e) => {
        e.preventDefault();
        let status = $("input[name='status']:checked").val() || 0;
        var temp_data = {
            'Type': $("#changeType").val(),
            'Status': status,
            'ProductType': $("#productType").val(),
            'Overstatus': $("input[name='overstatus']:checked").val(),
            'Changeitem': $("#changeitem").val(),
            'ControlNo': $("#TNScontrolNo").val(),
            'Model': $("#Model").val(),
            'Partno': $("#partno").val(),
            'Partname': $("#partname").val(),
            'Department': $("[name='dept']").val(),
            'Processname': $("#processname").val(),
            'Line': $("#line").val(),
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
                                    if(row.User_insert === SessionUser && (row.FullStatus == "Waiting" || row.FullStatus == "Request")){
                                        btn_badge = `success`;
                                        var editable = ``;
                                    }
                                    return `<div class="btn-group"><a href="Detail/?id=${row.Code}"><button type="button" name="detail" id="${row.Code}" class="btn btn-info btn-left btn-sm mb-1 mr-1" data-toggle="modal" data-target="#largeModal" >`+
                                    `<i class="fas fa-external-link-alt"></i></button></a><a href="Request/?id=${row.Code}"><button type="button" name="edit" id="${row.Code}" class="btn btn-`+
                                    btn_badge + ` btn-right btn-sm  mb-1" data-toggle="modal" data-target="#largeModal"` +
                                    editable + `>`+
                                    `<i class="fas fa-pen"></i>`+
                                    `</button></a></div>`;
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

                $("body, html").animate({
                    scrollTop: 700
                }, 100)
            },
            error: function () {
            }
        });
    });

});
