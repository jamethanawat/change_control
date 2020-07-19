var submit;
var submit_Related;
var submit_file;
var remove;
var sendmail;
var GetSummernote;
var test;
var TopicStatus = false;
var DepartmentLists;
$(document).ready(function () {
    //console.log("Request");
   console.log(DepartmentLists);
    
    $('.textareaIPP').summernote({
        height: 140,
    });
    $('.textareaSubject').summernote({
        height: 140,
    });
    $('.textareaDetails').summernote({
        height: 140,
    });
    $('.textareaTiming').summernote({
        height: 140,
    });

    var spanSubmit = $('.pTop');

    spanSubmit.on('click', function () {
        $(this).submit();
    });

    $.each(DepartmentLists, (key,val) => {
        console.log(val.Name);
        $(`#${val.Name}`).change((e) => {
                $(`.${val.Name}`).each(function() {
                    this.checked = (e.target.checked) ? true : false;
                });
        });
    
        $(`.${val.Name}`).click(function () {
            if ($(this).is(":checked")) {
                var isAllChecked = 0;
    
                $(`.${val.Name}`).each(function() {
                    if (!this.checked)
                        isAllChecked = 1;
                });
    
                if (isAllChecked == 0) {
                    $(`#${val.Name}`).prop("checked", true);
                }     
            }
            else {
                $(`#${val.Name}`).prop("checked", false);
            }
        });
    });
    

    



    //load edit from
    if ($("#MODE").attr("value") != "Insert") {

        DisableAll();
        //type
        if ($("#c_radio_type").attr("value") == "Internal") {

            $("#T_internal").prop("checked", true);

        } else {
            $("#T_external").prop("checked", true);
        } 
        //change item
        var change_item = $("#c_radio_ChangeItems").attr("value");
        if (change_item == 1) {
            $("#C_newpart").prop("checked", true);
        }
        else if (change_item == 2) {
            $("#C_material").prop("checked", true);
        }
        else if (change_item == 3) {
            $("#C_manufacturing_method").prop("checked", true);
        }
        else if (change_item == 4) {
            $("#C_inspection").prop("checked", true);
        }
        else if (change_item == 5) {
            $("#C_jig").prop("checked", true);
        }
        else if (change_item == 6) {
            $("#C_design").prop("checked", true);
        }
        else if (change_item == 7) {
            $("#C_newsupplier").prop("checked", true);
        }
        else if (change_item == 8) {
            $("#C_machine").prop("checked", true);
        }
        else if (change_item == 9) {
            $("#C_manufacturing_process").prop("checked", true);
        }
        else if (change_item == 10) {
            $("#C_packing").prop("checked", true);
        }
        else if (change_item == 11) {
            $("#C_die").prop("checked", true);
        }
        else if (change_item == 12) {
            $("#C_others").prop("checked", true);
        }
        // product type
        var product_type = $("#c_radio_ProductType").attr("value");
        if (product_type == 1) {
            $("#P_Meter2").prop("checked", true);
        }
        else if (product_type == 2) {
            $("#P_Meter4").prop("checked", true);
        }
        else if (product_type == 3) {
            $("#P_Agricul").prop("checked", true);
        }
        else if (product_type == 4) {
            $("#P_Fu").prop("checked", true);
        }
        else if (product_type == 5) {
            $("#P_DP").prop("checked", true);
        }
        else if (product_type == 6) {
            $("#P_Pointer").prop("checked", true);
        }
        else if (product_type == 7) {
            $("#P_Consumer").prop("checked", true);
        }
        else if (product_type == 8) {
            $("#P_Others").prop("checked", true);
        }
        if ($("#S-itemfile").attr("value") != "0") {
            $("#S-itemfile").removeClass('hidden');
        } else {
            $("#S-itemfile").addClass('hidden');
            $("#S-itemfile").html(0);
        }

        // all summernote
        $.ajax({
            url: GetSummernote,
            type: 'POST',
            data: JSON.stringify({ ID: $("#loading").attr("value") }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $.each(data, function (i, item) {
                    //console.log(item.APP);
                    //console.log(item.Subject);
                    //console.log(item.Detail);
                    //console.log(item.Timing);
                    $(".textareaIPP").summernote('code', "" + item.APP+"");
                    $(".textareaSubject").summernote('code', "" + item.Subject + "");
                    $(".textareaDetails").summernote('code', "" + item.Detail + "");
                    $(".textareaTiming").summernote('code', "" + item.Timing + "");
                });
             
             },
            error: function (err) {
                         swal({
                            title: "ERROR",
                            text: "Can't not connect database(E07)",
                            type: "warning",
                            confirmButtonColor: "#DD6B55",
                            confirmButtonText: "OK"
                        })
            }
        });
        //file


    }


    $("#btnEdit").click(function () {
   
        if ($('input[name="r2"]').prop("disabled")) {
            revision = $("#revision").html();
            revision = (parseInt(revision) + 1);
            $("#revision").html(revision);
            console.log(revision);
            EnableAll();
            if ($("#c_radio_APP").attr("value") == "False") {
                $("#App_N").prop("checked", true);
                $('.textareaIPP').summernote('disable');
            } else {
                $("#App_Y").prop("checked", true);
                $('.textareaIPP').summernote('enable');

            }
    }
       
       
    });
    $("#App_Y").click(function () {
        //console.log("y");
        $('.textareaIPP').summernote('enable');
        //console.log("N", $("#App_N").prop("checked"));
    });
    $("#App_N").click(function () {
        //console.log("N", $("#App_N").prop("checked"));
        $('.textareaIPP').summernote('disable');
    });
    $("#clear").click(function () {
        //console.log("clear");
        $("#D_P1").prop("checked", false);
        $("#D_P2").prop("checked", false);
        $("#D_P3A").prop("checked", false);
        $("#D_P3M").prop("checked", false);
        $("#D_P4").prop("checked", false);

        $("#D_P5").prop("checked", false);
        $("#D_P6").prop("checked", false);
        $("#D_P7").prop("checked", false);
        $("#D_IT").prop("checked", false);
        $("#D_MKT").prop("checked", false);

        $("#D_PC1").prop("checked", false);
        $("#D_PC2").prop("checked", false);
        $("#D_PCH1").prop("checked", false);
        $("#D_PCH2").prop("checked", false);
        $("#D_PE1").prop("checked", false);

        $("#D_PE2").prop("checked", false);
        $("#D_PE2SMT").prop("checked", false);
        $("#D_PE2PCB").prop("checked", false);
        $("#D_PE2MT").prop("checked", false);
        $("#D_QCIN1").prop("checked", false);
        $("#D_QCIN2").prop("checked", false);

        $("#D_QCIN3").prop("checked", false);
        $("#D_QCFI1").prop("checked", false);
        $("#D_QCFI2").prop("checked", false);
        $("#D_QCFI3").prop("checked", false);
        $("#D_QCNMF1").prop("checked", false);
        $("#D_QCNMF2").prop("checked", false);

        $("#D_QCNMF3").prop("checked", false);
        $("#D_QC1").prop("checked", false);
        $("#D_QC2").prop("checked", false);
        $("#D_QC3").prop("checked", false);
        $("#D_PE1PROCESS").prop("checked", false);
        $("#D_PE2PROCESS").prop("checked", false);
   
    });
    $("#approved").click(function () {
       
        $("#loading").removeClass('hidden');
        var modeapproved = $("#approved").attr("value").toString();
        var type = "";
        var changeitem;
        var producttype;
        var department = [];
        var txtfile = [];
       

        //IN or EX
        if ($("#T_internal").prop("checked")) {
            type = "Internal";
        }
        else {
            type = "External";
        }
        // Change item
        if ($("#C_newpart").prop("checked")) {
            changeitem = 1;
        }
        else if ($("#C_material").prop("checked")){
            changeitem = 2;
        }
        else if ($("#C_manufacturing_method").prop("checked")) {
            changeitem = 3;
        }
        else if ($("#C_inspection").prop("checked")) {
            changeitem = 4;
        }
        else if ($("#C_jig").prop("checked")) {
            changeitem = 5;
        }
        else if ($("#C_design").prop("checked")) {
            changeitem = 6;
        }
        else if ($("#C_newsupplier").prop("checked")) {
            changeitem = 7;
        }
        else if ($("#C_machine").prop("checked")) {
            changeitem = 8;
        }
        else if ($("#C_manufacturing_process").prop("checked")) {
            changeitem = 9;
        }
        else if ($("#C_packing").prop("checked")) {
            changeitem = 10;
        }
        else if ($("#C_die").prop("checked")) {
            changeitem = 11;
        }
        else if ($("#C_others").prop("checked")) {
            changeitem = 12;
        }

        //Product type
        if ($("#P_Meter2").prop("checked")) {
            producttype = 1;
        }
        else if ($("#P_Meter4").prop("checked")) {
            producttype = 2;
        }
        else if ($("#P_Agricul").prop("checked")) {
            producttype = 3;
        }
        else if ($("#P_Fu").prop("checked")) {
            producttype = 4;
        }
        else if ($("#P_DP").prop("checked")) {
            producttype = 5;
        }
        else if ($("#P_Pointer").prop("checked")) {
            producttype = 6;
        }
        else if ($("#P_Consumer").prop("checked")) {
            producttype = 7;
        }
        else if ($("#P_Others").prop("checked")) {
            producttype = 8;
        }

        //File
        var files = $("#files").get(0).files;
        var fileData = new FormData();

        for (var i = 0; i < files.length; i++) {
            console.log(files);
            fileData.append('files', files[i]);
        }

        // Department
        if ($("#D_P1").prop("checked")) {
            department[0] = 1;
        }
        else  {
            department[0] = 0;
        }
        if ($("#D_P2").prop("checked")) {
            department[1] = 1;
        }
        else {
            department[1] = 0;
        }
        if ($("#D_P3A").prop("checked")) {
            department[2] = 1;
        }
        else {
            department[2] = 0;
        }
        if ($("#D_P3M").prop("checked")) {
            department[3] = 1;
        }
        else {
            department[3] = 0;
        }
        if ($("#D_P4").prop("checked")) {
            department[4] = 1;
        }
        else {
            department[4] = 0;
        }



        if ($("#D_P5").prop("checked")) {
            department[5] = 1;
        }
        else {
            department[5] = 0;
        }
        if ($("#D_P6").prop("checked")) {
            department[6] = 1;
        }
        else {
            department[6] = 0;
        }
        if ($("#D_P7").prop("checked")) {
            department[7] = 1;
        }
        else {
            department[7] = 0;
        }
        if ($("#D_IT").prop("checked")) {
            department[8] = 1;
        }
        else {
            department[8] = 0;
        }
        if ($("#D_MKT").prop("checked")) {
            department[9] = 1;
        }              
        else {         
            department[9] = 0;
        }




        if ($("#D_PC1").prop("checked")) {
            department[10] = 1;
        }
        else {
            department[10] = 0;
        }
        if ($("#D_PC2").prop("checked")) {
            department[11] = 1;
        }
        else {
            department[11] = 0;
        }
        if ($("#D_PCH1").prop("checked")) {
            department[12] = 1;
        }
        else {
            department[12] = 0;
        }
        if ($("#D_PCH2").prop("checked")) {
            department[13] = 1;
        }
        else {
            department[13] = 0;
        }
        if ($("#D_PE1").prop("checked")) {
            department[14] = 1;
        }
        else {
            department[14] = 0;
        }



        if ($("#D_PE2").prop("checked")) {
            department[15] = 1;
        }
        else {
            department[15] = 0;
        }
        if ($("#D_PE2SMT").prop("checked")) {
            department[16] = 1;
        }
        else {
            department[16] = 0;
        }
        if ($("#D_PE2PCB").prop("checked")) {
            department[17] = 1;
        }
        else {
            department[17] = 0;
        }
        if ($("#D_PE2MT").prop("checked")) {
            department[18] = 1;
        }
        else {
            department[18] = 0;
        }
        if ($("#D_QCIN1").prop("checked")) {
            department[19] = 1;
        }
        else {
            department[19] = 0;
        }
        if ($("#D_QCIN2").prop("checked")) {
            department[20] = 1;
        }
        else {
            department[20] = 0;
        }



        if ($("#D_QCIN3").prop("checked")) {
            department[21] = 1;
        }
        else {
            department[21] = 0;
        }
        if ($("#D_QCFI1").prop("checked")) {
            department[22] = 1;
        }
        else {
            department[22] = 0;
        }

        if ($("#D_QCFI2").prop("checked")) {
            department[23] = 1;
        }
        else {
            department[23] = 0;
        }
        if ($("#D_QCFI3").prop("checked")) {
            department[24] = 1;
        }
        else {
            department[24] = 0;
        }
        if ($("#D_QCNMF1").prop("checked")) {
            department[25] = 1;
        }
        else {
            department[25] = 0;
        }
        if ($("#D_QCNMF2").prop("checked")) {
            department[26] = 1;
        }
        else {
            department[26] = 0;
        }



        if ($("#D_QCNMF3").prop("checked")) {
            department[27] = 1;
        }
        else {
            department[27] = 0;
        }
        if ($("#D_QC1").prop("checked")) {
            department[28] = 1;
        }
        else {
            department[28] = 0;
        }
        if ($("#D_QC2").prop("checked")) {
            department[29] = 1;
        }
        else {
            department[29] = 0;
        }
        if ($("#D_QC3").prop("checked")) {
            department[30] = 1;
        }
        else {
            department[30] = 0;
        }
        if ($("#D_PE1PROCESS").prop("checked")) {
            department[31] = 1;
        }
        else {
            department[31] = 0;
        }
        if ($("#D_PE2PROCESS").prop("checked")) {
            department[32] = 1;
        }
        else {
            department[32] = 0;
        }
        //console.log(department);
        var Odepartment = {

            "Value": department
        };
        var txtAPP = "";
        var CK_ERR = 0;
        if ($("#App_N").prop("checked")) {

            txtAPP = "";
        }
        else
        {
            txtAPP = $(".textareaIPP").val();
        }

        let ul = $('#listshow');
        let length = ul.children().length;
        for (var i = 0; i < length; i++) {
            txtfile.push($('#txt-file' + i + '').val());
            //console.log("value-text", $('#txt-file' + i + '').val());
        }
        var Otxtfile = {

            "value": txtfile
        };
        //check file edit or Insert
        if ($("#files").get(0).files.length == 0 && txtfile.length > 0) {
            modeapproved = "Edit-EditFile";
           //edit discription 
        } else {
          //newfile
        }
      

        $.ajax({
            type: "POST",
            //async: false,
            url: submit,     
            data: JSON.stringify({
                Mode: modeapproved,
                Type: type,
                Changeitem: changeitem,
                Producttype: producttype,
                revision: $("#revision").html(),
                Model: $("#P_model").val(),
                Partno: $("#P_partno").val(),
                Partname: $("#P_partname").val(),
                Processname: $("#P_processname").val(),
                App: txtAPP,
                Subject: $(".textareaSubject").val(),
                Detail: $(".textareaDetails").val(),
                Timing: $(".textareaTiming").val(),
            }),
            dataType: 'json',
            contentType: 'application/json',
            success: function (data) {
                if (data.code == 1) {
                    console.log("good");
                    $.ajax({
                        type: "POST",
                        //async: false,
                        url: submit_Related,     
                        data: JSON.stringify({
                            department: Odepartment,
                            TxtFile: Otxtfile,
                        }),
                        dataType: 'json',
                        contentType: 'application/json',
                        success: function (data) {
                            if (data.code > 0) {
                                $.ajax({
                                    url: submit_file,
                                    //async: false,
                                    type: 'POST',
                                    data: fileData,
                                    cache: false,
                                    processData: false,
                                    contentType: false,
                                    success: function (data) {
                                        if (data.code != "-1") {
                                            $("#loading").addClass('hidden');
                                            swal("Success \n NO. " + data.code + "", "Complete Transaction", "success");
                                            DisableAll();
                                            SendMail();
                                        }
                                        else
                                        {
                                            Remove();
                                            swal({
                                                title: "ERROR",
                                                text: "Can't not connect database(E06)",
                                                type: "warning",
                                                confirmButtonColor: "#DD6B55",
                                                confirmButtonText: "OK"
                                            })

                                        }

                                    },
                                    error: function () {
                                        Remove();
                                        swal({
                                            title: "ERROR",
                                            text: "Can't not connect database(E01)",
                                            type: "warning",
                                            confirmButtonColor: "#DD6B55",
                                            confirmButtonText: "OK"
                                        })
                                    }
                                });
                            }
                            else {
                                Remove();
                                swal({
                                    title: "ERROR",
                                    text: "Can't not connect database(E05)",
                                    type: "warning",
                                    confirmButtonColor: "#DD6B55",
                                    confirmButtonText: "OK"
                                })
                            }
                        },
                        error: function () {
                            Remove();

                            swal({
                                title: "ERROR",
                                text: "Can't not connect database(E02)",
                                type: "warning",
                                confirmButtonColor: "#DD6B55",
                                confirmButtonText: "OK"
                            })
                        }
                    });
                }
                else {
                    Remove();
                    swal({
                        title: "ERROR",
                        text: "Can't not connect database(E03)",
                        type: "warning",
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "OK"
                    })
                }

            },
            error: function () {
                $("#loading").addClass('hidden');
                swal({
                    title: "ERROR",
                    text: "Can't not connect database(E04)",
                    type: "warning",
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: "OK"
                })
            }
        });
    });
    function EnableAll() {

        //$('input[name="r1"]').prop("disabled", false);
        $('input[name="r2"]').prop("disabled", false);
        $('input[name="r3"]').prop("disabled", false);

        $('#P_model').prop("disabled", false);
        $('#P_partno').prop("disabled", false);
        $('#P_partname').prop("disabled", false);
        $('#P_processname').prop("disabled", false);

        $('#c_radio_APP').prop("disabled", false);
        $('input[name="r4"]').prop("disabled", false);
        $('#btnAttc').prop("disabled", false);

        $('#D_P1').prop("disabled", false);
        $('#D_P2').prop("disabled", false);
        $('#D_P3A').prop("disabled", false);
        $('#D_P3M').prop("disabled", false);
        $('#D_P4').prop("disabled", false);
        $('#D_P5').prop("disabled", false);
        $('#D_P6').prop("disabled", false);
        $('#D_P7').prop("disabled", false);
        $('#D_IT').prop("disabled", false);
        $('#D_MKT').prop("disabled", false);

        $('#D_PC1').prop("disabled", false);
        $('#D_PC2').prop("disabled", false);
        $('#D_PCH1').prop("disabled", false);
        $('#D_PCH2').prop("disabled", false);
        $('#D_PE1').prop("disabled", false);
        $('#D_PE2').prop("disabled", false);
        $('#D_PE2SMT').prop("disabled", false);
        $('#D_PE2PCB').prop("disabled", false);
        $('#D_PE2MT').prop("disabled", false);
        $('#D_QCIN1').prop("disabled", false);
        $('#D_QCIN2').prop("disabled", false);
        $('#D_QCIN3').prop("disabled", false);
        $('#D_QCFI1').prop("disabled", false);
        $('#D_QCFI2').prop("disabled", false);
        $('#D_QCFI3').prop("disabled", false);
        $('#D_QCNMF1').prop("disabled", false);
        $('#D_QCNMF2').prop("disabled", false);
        $('#D_QCNMF3').prop("disabled", false);
        $('#D_QC1').prop("disabled", false);
        $('#D_QC2').prop("disabled", false);
        $('#D_QC3').prop("disabled", false);
        $('#D_PE1PROCESS').prop("disabled", false);
        $('#D_PE2PROCESS').prop("disabled", false);
        $('#clear').prop("disabled", false);
        $('#approved').prop("disabled", false);
        $('.textareaIPP').summernote('enable');
        $('.textareaSubject').summernote('enable');
        $('.textareaDetails').summernote('enable');
        $('.textareaTiming').summernote('enable');
    }
    function DisableAll() {

        $('input[name="r1"]').prop("disabled", true);
        $('input[name="r2"]').prop("disabled", true);
        $('input[name="r3"]').prop("disabled", true);

        $('#P_model').prop("disabled", true);
        $('#P_partno').prop("disabled", true);
        $('#P_partname').prop("disabled", true);
        $('#P_processname').prop("disabled", true);

        $('#c_radio_APP').prop("disabled", true);
        $('input[name="r4"]').prop("disabled", true);
        $('#btnAttc').prop("disabled", true);

        $('#D_P1').prop("disabled", true);
        $('#D_P2').prop("disabled", true);
        $('#D_P3A').prop("disabled", true);
        $('#D_P3M').prop("disabled", true);
        $('#D_P4').prop("disabled", true);
        $('#D_P5').prop("disabled", true);
        $('#D_P6').prop("disabled", true);
        $('#D_P7').prop("disabled", true);
        $('#D_IT').prop("disabled", true);
        $('#D_MKT').prop("disabled", true);

        $('#D_PC1').prop("disabled", true);
        $('#D_PC2').prop("disabled", true);
        $('#D_PCH1').prop("disabled", true);
        $('#D_PCH2').prop("disabled", true);
        $('#D_PE1').prop("disabled", true);
        $('#D_PE2').prop("disabled", true);
        $('#D_PE2SMT').prop("disabled", true);
        $('#D_PE2PCB').prop("disabled", true);
        $('#D_PE2MT').prop("disabled", true);
        $('#D_QCIN1').prop("disabled", true);
        $('#D_QCIN2').prop("disabled", true);
        $('#D_QCIN3').prop("disabled", true);
        $('#D_QCFI1').prop("disabled", true);
        $('#D_QCFI2').prop("disabled", true);
        $('#D_QCFI3').prop("disabled", true);
        $('#D_QCNMF1').prop("disabled", true);
        $('#D_QCNMF2').prop("disabled", true);
        $('#D_QCNMF3').prop("disabled", true);
        $('#D_QC1').prop("disabled", true);
        $('#D_QC2').prop("disabled", true);
        $('#D_QC3').prop("disabled", true);
        $('#D_PE1PROCESS').prop("disabled", true);
        $('#D_PE2PROCESS').prop("disabled", true);
        $('#clear').prop("disabled", true);
        $('#approved').prop("disabled", true);
        $('.textareaIPP').summernote('disable');
        $('.textareaSubject').summernote('disable');
        $('.textareaDetails').summernote('disable');
        $('.textareaTiming').summernote('disable');
    }
    function Remove() {
        $("#loading").addClass('hidden');
        $.ajax({
            url: remove,
            type: 'POST',
            data: JSON.stringify({
                revision: $("#revision").html(),          
            }),
            dataType: 'json',
            contentType: 'application/json',
            success: function (data) {

            },
            error: function () {
            }
        });
    }
    function  SendMail(type) {

        $.ajax({
            url: sendmail,
            type: 'POST',
            data: JSON.stringify({
                Type: type,
            }),
            dataType: 'json',
            contentType: 'application/json',
            success: function (data) {

            },
            error: function () {
            }
        });
    }
    $("#test").click(function () {

        console.log($("#revision").html());
        SendMail("Internal");
        //var txt = $(".textarea2").val();
        //console.log(txt);
        let ul = $('#listshow');              
        let length = ul.children().length;
        console.log("ul", ul);
        console.log("length", length);
        for (var i = 0; i < length; i++) {
           
            console.log("value-text", $('#txt-file' + i +'').val());
        }
        var Odepartment = {

            "Value": 1
        };
        var Odepartment2 = {

            "Value": 2
        };

        var files = $("#files").get(0).files;
        var fileData = new FormData();

        for (var i = 0; i < files.length; i++) {
            fileData.append('files', files[i]);
        }
        
        var code = "5555";


        //$.ajax({
        //    url: submit_file,
        //    type: 'POST',
        //    data: fileData,
        //    cache: false,
        //    processData: false,
        //    contentType: false,
        //    success: function (color) {
                
        //    },
        //    error: function () {
        //        alert('Error occured');
        //    }
        //});
        swal("Success \n Change Control NO. " + code + "", "Complete Transaction", "success");
        //swal({
        //    title: "success",
        //    titleText:"5565",
        //    text: "Complete Transaction",
        //    type: "success",
        //    confirmButtonColor: "#218838",
        //    confirmButtonText: "OK"
        //})
        //$.ajax({
        //    type: "POST",
        //    //async: false,
        //    url: submit_department,
        //    data: JSON.stringify({
        //        department: Odepartment,
        //        department2: Odepartment2,
        //    }),
        //    dataType: 'json',
        //    contentType: 'application/json',
        //    success: function (data) {

        //    },
        //    error: function () {
        //        alert('Error occured');
        //    }
        //});

    });

    $("#btnAttc").click(function () {
        console.log("click");
        $('#exampleModal').modal("show");
    });
    $("#upload-label").click(function (evt) {
        console.log("name", event.target.nodeName);
        console.log("id", event.target.ID);

        if (evt.target.name == "txt-file" || event.target.nodeName == "LI" || event.target.nodeName == "BUTTON") {

            return;
        } else {
        $("#files").html("");
        $('#files').click();
        return;
        }

    });
    $("#clearfile").click(function () {
        //console.log("click3");
        $("#files").val('').clone(true)
        $("#listfile").addClass('hidden');
        $("#listshow").remove();

        $("#S-itemfile").addClass('hidden');
        $("#S-itemfile").html(0);
    });
    $("input[name=files]").change(function () {     
        var html = '';
        $("#listshow").remove();
        html = '<ul id="listshow" class="list-group list-group-flush">';
        for (var i = 0; i < $(this).get(0).files.length; ++i) {
            //names.push($(this).get(0).files[i].name);
            html += '<li class="list-group-item"><div class="row"><span class="col-6 pTop"> ' + $(this).get(0).files[i].name + '</span>';
            html += '<div class="col-6"><input type="text" placeholder="Description .." id="txt-file' + i +'" name="txt-file" class="form-control"></li></div></div>';

            //html += '<li class="list-group-item">' + $(this).get(0).files[i].name + '</li>';
         
        }
        html += ' </ul>';

        if ($(this).get(0).files.length == 0) {
            
            $("#listfile").addClass('hidden');
            $("#S-itemfile").addClass('hidden');
        } else {
            $("#S-itemfile").removeClass('hidden');
            $("#S-itemfile").html($(this).get(0).files.length);
            $("#listfile").removeClass('hidden');
            $("#listfile").append(html);
        }     
    });
});

