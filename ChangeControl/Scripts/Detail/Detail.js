var ReviewStatus = false;
var file_list = [];
var file_list_alt = [];
var file_list_rd = [];
var due_date;
var rsm_id = 0;
var resubmit_formIsEmpty = true;
var TrialIsEmpty = true;
var rsm_related_list = [];
var new_related_list = [];
var rv_submit;

/* -------------------------------------------------------------------------- */
/*                          For check radio and input                         */
/* -------------------------------------------------------------------------- */

var findDuplicates = arr => arr.filter((item, index) => arr.indexOf(item) != index)
var removeDuplicates = (names) => names.filter((v,i) => names.indexOf(v) === i)
var new_rv = [];

$(() => {
    var rv_form = $('form#review').serializeArray();
    rv_form.forEach(function(rv){
        new_rv.push(rv.name.split("-")[1]);
    });

    if(topic_status == "7" || !isQC || !isReview){
        $(".zoom-fab#change_status").addClass("hide-fab");
    }

/* ---------------------------------------------------------------------------------------------- */
/*                                            Intro.js                                            */
/* ---------------------------------------------------------------------------------------------- */
    if(isResubmitted && !localStorage.getItem(`${us_id}-reply_submit`)){
        introJs().onbeforechange(() => {
            $('[href="#timeline"]').click()
        }).onexit(() => {
            localStorage.setItem(`${us_id}-reply_submit`, true);
        }).start();
    }
/* ---------------------------------------------------------------------------------------------- */
/*                                            For notyf                                           */
/* ---------------------------------------------------------------------------------------------- */

    if(Resubmitable){
        createNotification({ID : "resubmit", Type : "Warning", Message : "หากแผนกใดๆ Upload เอกสารไม่ครบถ้วน คุณสามารถ Resubmit แผนกดังกล่าวเพื่อขอเอกสารเพิ่มเติม <br><br> กดกากบาท (X) หากรับทราบแล้ว"})
    }

/* -------------------------------------------------------------------------- */
/*                                Reject Topic                                */
/* -------------------------------------------------------------------------- */

$("#reject").click(() => {
        swal({
            title: "Warning", 
            text: "Do you want to reject this Topic?", 
            content: "input",
            buttons : [true,true],
            icon:"warning",
        }).then((res) => {
            if(res != null){
                if(res.trim().length != 0){
                    $.post(RejectTopicPath, {topic_status:topic_status, topic_dept:topic_dept, topic_code:topic_code,desc:res}, (result) => { 
                        if(result.status == "success" && result.mail != null && result.mail != ""){
                            $.post(GenerateMailPath,{ 'mode': result.mail, 'dept': result.dept, 'topic_code':topic_code, }).fail((error) => {
                                console.error(error);
                                swal("Error", "Cannot send email to Requestor, Please try again", "error");
                                return;
                            })
                        }
                        swal("Success", "Reject Success", "success").then(location.reload());
                    }).fail( function(xhr, textStatus, errorThrown) {
                        console.error(xhr.responseText);
                        swal("Something wrong", "Please contact admin", "error");
                    });
                }else{
                    swal("Something wrong", "Please enter the text", "error");
                }
            }
        });
    });

/* -------------------------------------------------------------------------- */
/*                                  Go to top                                 */
/* -------------------------------------------------------------------------- */

    $('[data-toggle="tooltip"]').tooltip();
    var btn = $('#Top');
    $(window).scroll(() => {
        if ($(window).scrollTop() > 600) {
            btn.addClass('show');
        } else {
            btn.removeClass('show');
        }
    });

    btn.on('click', function (e) {
        e.preventDefault();
        $('html, body').animate({ scrollTop: 0 }, '300');
    });

/* -------------------------------------------------------------------------- */
/*                             Resubmit's Validate                            */
/* -------------------------------------------------------------------------- */

var rsm_validator = $('#resubmit_form').validate({
    rules: {
        desc: {required: true,},
        due_date: {required: true,date: true}
    },
    messages: {
        desc: {required: "Please enter a description",},
        due_date: {required: "Please enter a due date",date: "Please enter a valid date"}
    },
    errorElement: 'span',
    errorPlacement: function (error, element) {
      error.addClass('invalid-feedback');
      element.closest(".col-form-label").append(error);
      $(".btn-success").prop('disabled', true);
    },
    highlight: function (element, errorClass, validClass) {$(element).addClass('is-invalid');},
    unhighlight: function (element, errorClass, validClass) {$(element).removeClass('is-invalid');}
});

/* -------------------------------------------------------------------------- */
/*                           Resubmit's Wizard modal                          */
/* -------------------------------------------------------------------------- */

    $("#resubmit_modal").modalWizard();
    $("#resubmit_modal").on("navigate", (e, navDir, stepNumber) => {
        if($("#resubmit_modal").attr("data-current-step") == 3){
            let quick_form = $("form#resubmit_form").serializeArray();
            console.log(quick_form);

            let related = due_date = desc = "";
            
            quick_form.forEach(item => {
                if(item.value == "1"){
                    rsm_related_list.push(item.name);
                    related = (related == "") ? item.name : related + " , " + item.name;
                }else if(item.name == "desc"){
                    desc = item.value;
                }else if(item.name == "due_date"){
                    due_date = item.value;
                }
            });

            moment.locale("th");
            $("#related_date").val(moment(due_date,"DD-MM-YYYY").format('LL'));
            $("#related_dept").html(related);
            $("#related_desc").html(desc);
            console.log("related: ",related);
        }else{
            $(".btn-success").prop('disabled', true);
            if(navDir == "prev"){
                relatedValidate();
            }else{
                if(!resubmit_formIsEmpty){
                    $(".btn-success").prop('disabled', (rsm_validator.form()) ? false : true); 
                }
            }
        }
    });
    
/* -------------------------------------------------------------------------- */
/*                           Related's Wizard modal                           */
/* -------------------------------------------------------------------------- */

$("#related_rv").modalWizard();
$("#related_rv").on("navigate", (e, navDir, stepNumber) => {
    if($("#related_rv").attr("data-current-step") == 2){
        let new_related_string = "";
        new_related_list = [];
        $(".new_related:checked").each(function () {
            new_related_list.push(this.name);
        });
        new_related_list.forEach(item => {
            new_related_string = (new_related_string == "") ? item : new_related_string + " , " + item;
        });

        $("#new_related_dept").html(new_related_string);
        console.log("related: ",new_related_string);
    }else{
        // $(".btn-success").prop('disabled', true);
    }
});

/* -------------------------------------------------------------------------- */
/*                             Resubmit's Validate                            */
/* -------------------------------------------------------------------------- */

    $("#resubmit_form [name='desc'], #resubmit_form [name='due_date']").on("keydown keyup click change", () => {
        if($("#resubmit_form [name='desc']").val() != "" && $("#resubmit_form [name='due_date']").val() !== ""){
            resubmit_formIsEmpty = false;
        }else{
            resubmit_formIsEmpty = true;
        }
        if(!resubmit_formIsEmpty){
            $(".btn-success").prop('disabled', (rsm_validator.form()) ? false : true); 
        }
    });

/* -------------------------------------------------------------------------- */
/*                              Trial's Validate                              */
/* -------------------------------------------------------------------------- */

    var tr_validator = $('#Trial').validate({
        rules: { 
          tr_desc: {required: true,},
        },
        messages: {
          tr_desc: {required: "Please enter a description",},
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
          error.addClass('invalid-feedback');
          element.closest(".col-form-label").append(error);
          $(".btn-success").prop('disabled', true);
        },
        highlight: function (element, errorClass, validClass) {$(element).addClass('is-invalid');},
        unhighlight: function (element, errorClass, validClass) {$(element).removeClass('is-invalid');}
    });
    
    $("#Trial [name='tr_desc']").on("keydown keyup", () => {
        $("#tr_submit").prop('disabled', (tr_validator.form()) ? false : true); 
    });

    var cf_validator = $('#Confirm').validate({
        rules: { 
          cf_desc: {required: true,},
        },
        messages: {
          cf_desc: {required: "Please enter a description",},
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
          error.addClass('invalid-feedback');
          element.closest(".col-form-label").append(error);
          $(".btn-success").prop('disabled', true);
        },
        highlight: function (element, errorClass, validClass) {$(element).addClass('is-invalid');},
        unhighlight: function (element, errorClass, validClass) {$(element).removeClass('is-invalid');}
    });
    
    $("#Confirm [name='cf_desc']").on("keydown keyup", () => {
        $("#cf_submit").prop('disabled', (cf_validator.form()) ? false : true); 
    });
    
/* -------------------------------------------------------------------------- */
/*                        Resubmit Department checkbox                        */
/* -------------------------------------------------------------------------- */

    $.each(DepartmentLists, (key,val) => {
        if($(`.rs-${val.Name}`).length == $(`.rs-${val.Name}:checked`).length){
            $(`#rs-${val.Name}`).prop('checked', true);
        relatedValidate();
    }
        $(`#rs-${val.Name}`).change(function(e) {
                $(`.rs-${val.Name}`).each(function() {
                    this.checked = (e.target.checked) ? true : false;
                });
            relatedValidate();
        });

        $(`.rs-${val.Name}`).click(function () {
            if($(this).is(":checked")) {
                var isAllChecked = 0;

                $(`.rs-${val.Name}`).each(function() {
                    if (!this.checked) isAllChecked = 1;
                });

                if(isAllChecked == 0) {
                    $(`#rs-${val.Name}`).prop("checked", true);
                }     
            }else{
                $(`#rs-${val.Name}`).prop("checked", false); 
            }
            relatedValidate();
        });
    });

/* -------------------------------------------------------------------------- */
/*                        Related Department checkbox                         */
/* -------------------------------------------------------------------------- */

$.each(DepartmentLists, (key,val) => {
    if($(`.rl-${val.Name}`).length == $(`.rl-${val.Name}:checked`).length){
            $(`#rl-${val.Name}`).prop("disabled", true);
            $(`#rl-${val.Name}`).prop('checked', true);
    relatedValidate();
}
    $(`#rl-${val.Name}`).change(function(e) { //Group checkbox
            $(`.rl-${val.Name}`).each(function() { //Sub checkbox
                if(this.disabled == false){ //If sub checkbox is disabled
                    this.checked = (e.target.checked) ? true : false;
                }
            });
        relatedValidate();
    });

    $(`.rl-${val.Name}`).click(function () { //Sub checkbox
        if($(this).is(":checked")) {
            var isAllChecked = 0;

            $(`.rl-${val.Name}`).each(function() { 
                if (!this.checked) isAllChecked = 1;
            });

            if(isAllChecked == 0) {
                $(`#rl-${val.Name}`).prop("checked", true);
            }     
        }else{
            $(`#rl-${val.Name}`).prop("checked", false); 
        }
        relatedValidate();
    });
});

    function relatedValidate(){
        checkbox_dept = $('input:checkbox.qForm.checkSingle:checked').length
        $(".rsm-next").prop('disabled', (checkbox_dept > 0) ? false : true);
    }

    $('[data-toggle="datepicker"]').datepicker({
        format: 'dd-mm-yyyy'
    });
    
/* -------------------------------------------------------------------------- */
/*                                Topic approve                               */
/* -------------------------------------------------------------------------- */

    $("#tp_approve").click(() => {
        swal({
            title: "Approve Topic", 
            text: "Do you want to approve this Topic?", 
            buttons : [true,true],
            icon:"warning",
        }).then((res) => {
            if(res){
                $.post(ApproveTopicPath,{topic_code:topic_code},() => {
                    var promises = [];
                    promises.push($.post(GenerateMailPath,{
                        'mode':(topic_code.substring(0,2) == "EX") ? 'InformUser' : 'InformPE',
                        'topic_code':topic_code,
                        'dept':(topic_code.substring(0,2) == "IN") ? pe_audit : null,
                    }).fail((error) => {
                    console.error(error);
                    swal("Error", "Cannot send email to Requestor, Please try again", "error");
                    return;
                }));
                    swal("Success", "Approve Success", "success").then(location.reload());
                }).fail( function(xhr, textStatus, errorThrown) {
                    console.error(xhr.responseText);
                    swal("Something wrong", "Please contact admin", "error");
                });
            }
        });
    })
/* -------------------------------------------------------------------------- */
/*                             Submit review topic                            */
/* -------------------------------------------------------------------------- */

$("form#review").submit((e) => {
    rv_submit = true;
    let RadioNotValidate = checkRadioAndInput(new_rv,rv_submit);
    let InputNotValidate = checkInputRequired();
    if(RadioNotValidate || InputNotValidate){
        return; 
    }

    e.preventDefault();
    $('#loading').removeClass('hidden')
    let rv_form = SerializeReviewForm();
    $.post(InsertReviewPath, {topic_id:topic_id, topic_code:topic_code}, (result) => {
        if(result.mail != ""){
            $.post(GenerateMailPath,{ 'mode': result.mail, 'topic_code':topic_code, 'dept':result.dept, 'pos':result.pos }).fail((error) => {
                console.error(error);
                swal("Error", "Cannot send email to Requestor, Please try again", "error");
                return;
            })
        }
        var promises = [];
        files = file_list;
        console.log("files",files);
        
        for(var index in files){
            files[index].file = files[index].detail.file;
            delete files[index].detail;
        }

        files.forEach(element => {
            var Data = new FormData();
            Data.append("file",element.file);
            Data.append("description",element.description);
            Data.append("code",topic_code);
                promises.push($.ajax({
                type: "POST",
                url: InsertFilePath,
                data: Data,
                cache: false,
                processData: false,
                contentType: false,
                error: function() {
                    swal("Error", "Upload file not success", "error");
                }
            }));
        });

        rv_form.forEach(element => {
            console.log(element);
            promises.push(
                $.post(InsertReviewItemPath, {
                    'status' : element.status,
                    'description' : element.desc,
                    'id' : element.id,
                },(data) => {
                    console.log('Inserted item');
                }).fail(() => {
                    alert('error handling here');
                })
            )
        });
        
        Promise.all(promises).then(() => {
            $('#loading').addClass('hidden')
            $("#ReviewSubmit").prop("disabled",true)
            swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
        })
    }).fail(() => {
        setTimeout(() => { location.reload(); }, 1500);
    });

});

/* -------------------------------------------------------------------------- */
/*                             Submit trial topic                             */
/* -------------------------------------------------------------------------- */

$("form#Trial").submit((e) => {
    e.preventDefault();
    $('#loading').removeClass('hidden')
        let trial_form = $("form#Trial").serializeArray();
        var promises = [];

        files = file_list;
        console.log("files",files);
        
        for(var index in files){
            files[index].file = files[index].detail.file;
            delete files[index].detail;
        }

        $.post(InsertTrialPath,{ desc: trial_form[0].value},(result) => {
            promises.push(files.forEach(element => {
                var Data = new FormData();
                Data.append("file",element.file);
                Data.append("description",element.description);
                Data.append("code",topic_code);
                $.ajax({
                    type: "POST",
                    url: InsertFileTrialPath,
                    data: Data,
                    cache: false,
                    processData: false,
                    contentType: false,
                    success: function () {
                        console.log('trial file uploaded');
                    },error: function() {
                        swal("Error", "Upload file not success", "error");
                    }
                });
            }));

            if(result.mail != ""){
                promises.push($.post(GenerateMailPath,{ 'mode': result.mail, 'topic_code':topic_code, 'dept':result.dept, 'pos':result.pos }).fail((error) => {
                    console.error(error);
                    swal("Error", "Cannot send email to Requestor, Please try again", "error");
                    return;
                }));
            }

            Promise.all(promises).then(() => {
                $('#loading').addClass('hidden')
                
                $("#trial_submit").prop("disabled",true)
                swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
            })
        }).fail(() => {
            swal("Error", "Trial is not succes, Please contact admin", "error");
            $('#loading').addClass('hidden')
        });
        
        
});

/* -------------------------------------------------------------------------- */
/*                            Submit confirm topic                            */
/* -------------------------------------------------------------------------- */

$("form#Confirm").submit((e) => {
    e.preventDefault();
    $('#loading').removeClass('hidden')
        let confirm_form = $("form#Confirm").serializeArray();
        var promises = [];

        files = file_list;
        console.log("files",files);
        
        for(var index in files){
            files[index].file = files[index].detail.file;
            delete files[index].detail;
        }

        $.post(InsertConfirmPath,{topic_id:topic_id, topic_code:topic_code , desc: confirm_form[0].value},(result) => {
            promises.push(files.forEach(element => {
                var Data = new FormData();
                Data.append("file",element.file);
                Data.append("description",element.description);
                Data.append("code",topic_code);
                $.ajax({
                    type: "POST",
                    url: InsertFileConfirmPath,
                    data: Data,
                    cache: false,
                    processData: false,
                    contentType: false,
                    success: function () {
                        console.log('confirm file uploaded');
                    },error: function() {
                        swal("Error", "Upload file not success", "error");
                    }
                });
            }));

            if(result.mail != ""){
                promises.push($.post(GenerateMailPath,{ 'mode': result.mail, 'topic_code':topic_code, 'dept':result.dept, 'pos':result.pos }).fail((error) => {
                    console.error(error);
                    swal("Error", "Cannot send email to Requestor, Please try again", "error");
                    return;
                }));
            }

            Promise.all(promises).then(() => {
                $('#loading').addClass('hidden')
                
                $("#cf_submit").prop("disabled",true)
                swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
            })
        }).fail(() => {
            swal("Error", "Confirm is not succes, Please contact admin", "error");
            $('#loading').addClass('hidden')
        });

        
});

/* -------------------------------------------------------------------------- */
/*                               Apply resubmit                               */
/* -------------------------------------------------------------------------- */
    $("form#resubmit_form").submit((e) => {
        e.preventDefault();
        let quick_form = $(".rsm_related_radio").serializeArray();
        for(x in quick_form){
            quick_form[x] = quick_form[x].name;
        }

        console.log(quick_form);
        $.post(InsertRelatedPath, {dept_list:quick_form}, () =>{
            console.log('Related created');
            $.post(RequestResubmitPath, $("#resubmit_form").serialize(), (res) =>{
                console.log('Resubmit created');
                if(res.code){
                    moment.locale('en');
                    $.post(GenerateMailPath,{ 'mode': 'RequestDocument', 'topic_code':topic_code, 'due_date': moment(due_date,"DD-MM-YYYY").format('D MMMM YYYY'), 'dept_arry': rsm_related_list, }).fail((error) => {
                        console.error(error);
                        swal("Error", "Cannot send email to Requestor, Please try again", "error");
                        return;
                    })
                    swal("Success", "Resubmit Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
                }else{
                    swal("Error", "Resubmit Not Success, Please Try Again", "error");
                }
            });
        });
    });

/* -------------------------------------------------------------------------- */
/*                               Apply related                                */
/* -------------------------------------------------------------------------- */

$("form#related_form").submit((e) => {
    e.preventDefault();

    let QC1 = $("input#rl-29").prop("checked");
    let QC2 = $("input#rl-30").prop("checked");
    let QC3 = $("input#rl-31").prop("checked");
        
    let PE1_Process = $("input#rl-32").prop("checked");
    let PE2_Process = $("input#rl-33").prop("checked");
    let P5_ProcessDesign = $("input#rl-44").prop("checked");
    let P6_ProcessDesign = $("input#rl-45").prop("checked");


    if(isInternal){
        if(!(PE1_Process || PE2_Process || P5_ProcessDesign || P6_ProcessDesign) || !(QC1 || QC2 || QC3)){ //Need to select PE_Process or QC as Auditor at lease one
            swal("Warning", "Please select PE_Process and QC at least one", "warning");
            return;
        }else if(Number(QC1) + Number(QC2) + Number(QC3) != 1 && Number(PE1_Process) + Number(PE2_Process) + Number(P5_ProcessDesign) + Number(P6_ProcessDesign) != 1  == false){ //When select QC and PE_Process more than one
            swal("Warning", "Please select one QC and one PE_Process", "warning");
            return
        }else if(Number(QC1) + Number(QC2) + Number(QC3) != 1 ){ //When select QC more than one
            swal("Warning", "Please select one QC", "warning");
            return
        }else if(Number(PE1_Process) + Number(PE2_Process) + Number(P5_ProcessDesign) + Number(P6_ProcessDesign) != 1   ){ //When select PE_Process more than one
            swal("Warning", "Please select one PE_Process", "warning");
            return
        }
    }else if(isExternal){
        if(!(QC1 || QC2 || QC3)){ //Need to select QC as Auditor at lease one
            swal("Warning", "Please select QC at least one", "warning");
            return;
        }else if(Number(QC1) + Number(QC2) + Number(QC3) != 1 ){ //When select QC more than one
            swal("Warning", "Please select one QC", "warning");
            return
        }
    }

    let new_dept_related = $(".new_related").serializeArray();
    for(x in new_dept_related){
        new_dept_related[x] = new_dept_related[x].name;
    }

    $(".new_related").attr("disabled",false);
    
    let new_all_related = $(".new_related").serializeArray();
    for(x in new_all_related){
        new_all_related[x] = new_all_related[x].name;
    }
    
    console.log(new_all_related);
    $.post(InsertRelatedPath, {dept_list : new_all_related}, () =>{
        console.log('Related created');
        $.post(UpdateTopicRelatedPath, {topic_code:topic_code},(res) => {
            if(res.status == "success"){
                if(topic_status == "8"){
                    $.post(GenerateMailPath,{ 'mode': 'InformUser', 'topic_code':topic_code, 'dept_arry': new_dept_related, }).fail((error) => {
                        console.error(error);
                        swal("Error", "Cannot send email to Related user, Please try again", "error");
                        return;
                    });
                }
                swal("Success", "Update related complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
            }
        });
    });
});

/* -------------------------------------------------------------------------- */
/*                              Response resubmit                             */
/* -------------------------------------------------------------------------- */

    $("#submit_reply_form").click((e) => {
        e.preventDefault();
        $('#loading').removeClass('hidden')
            let form_response = $("form.reply_form").serializeArray();
            var promises = [];

            files = file_list_rd;
            console.log("files",files);
            
            for(var index in files){
                files[index].file = files[index].detail.file;
                delete files[index].detail;
            }

            promises.push($.post(InsertResponsePath,{ desc: form_response[0].value, resubmit_id: rsm_id},() => {
                console.log('Inserted item');
                files.forEach(element => {
                    var Data = new FormData();
                    Data.append("file",element.file);
                    Data.append("description",element.description);
                    Data.append("code",topic_code);
                    promises.push($.ajax({
                        type: "POST",
                        url: InsertFileResponsePath,
                        data: Data,
                        cache: false,
                        processData: false,
                        contentType: false,
                        error: function() {
                            swal("Error", "Upload file not success", "error");
                        }
                    }));
                });
            }).fail(() => {
                swal("Error", "Reply not success", "error");
            }));

            Promise.all(promises).then(() => {
                $('#loading').addClass('hidden')
                
                $("#ResponseSubmit").prop("disabled",true)
                $("#reply_modal").modal("hide")
                swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
            })
    });


/* -------------------------------------------------------------------------- */
/*                             Change topic status                            */
/* -------------------------------------------------------------------------- */

    $(".zoom-fab#change_status").click(() => {
        var change_status = "Change status from";
        var new_status = 0;
        if(topic_status == "8"){
            change_status += " Review to Trial";
            new_status = 9;
        }else if(topic_status == "9"){
            change_status += " Trial to Confirm";
            new_status = 10;
        }else if(topic_status == "10"){
            change_status += " Confirm to Close";
            new_status = 11;
        }
        swal({
            title: "Change Status", 
            buttons: [true,"Confirm"],
            text: change_status, 
            icon:"warning",
        }).then((isChanged) => {
            if(isChanged){
                $.post(UpdateTopicStatusPath, {topic_code:topic_code,status:new_status}, (data) => {
                    if(data){
                        swal("Success", "Change Status Success", "success").then(location.reload());
                    }else{
                        swal("Error", "User Password Not Correct", "error");
                    }
                },"json");
            }
        });
    });

/* ---- Clear file in filepond when changing from Resubmit topic to topic --- */
    var reply_id = 0;
    $(".reply_modal").click((e) => {
        let new_reply_id = e.target.id.split("-")[1];
        if(new_reply_id != reply_id){
            console.log("id not matched");
            $(".reply_form textarea").val('');
            reply_id = new_reply_id;
            if(file_list_rd.length > 0){
                file_list_rd.forEach(e => {
                    alt_removeFile(e.detail.id);
                });
                file_list_rd = [];
            }
            $("form.reply_form").attr("id", new_reply_id);
        }else{
            console.log("id matched");
        }
    });

/* ----------------- Disable textbox when radio is not check ---------------- */
$('form#review').on('keyup change paste', 'input, select, textarea', (e) => {
    checkRadioAndInput(new_rv,rv_submit);
    /* --------------- and check is valid or not after submit once -------------- */
        if(rv_submit) checkInputRequired();
    });
    checkRadioAndInput(new_rv,rv_submit);
});

function SetResubmitID(rsm_id){
    this.rsm_id = rsm_id;
}


/* -------------------------------------------------------------------------- */
/*                 Validate input that hvae same id with radio                */
/* -------------------------------------------------------------------------- */

function checkRadioAndInput(radio_input_list,submit_once){
    var isNotfilled = false;
    removeDuplicates(findDuplicates(radio_input_list)).forEach(d => {
        $(`[name="desc-${d}"]`).attr("disabled",($(`[name="rd-${d}"]:checked`).val() == 1) ? false : true);
        if($(`[name="rd-${d}"]:checked`).val() == 1 && $(`[name="desc-${d}"]`).val().length < 1){
            isNotfilled = true;
            if(submit_once) $(`[name="desc-${d}"]`).addClass("is-invalid");
        }else if($(`[name="rd-${d}"]:checked`).val() == 0){
            $(`[name="desc-${d}"]`).val("");
            $(`[name="desc-${d}"]`).removeClass("is-invalid");
        }else{
            $(`[name="desc-${d}"]`).removeClass("is-invalid");

        }
    });

/* ----------------------------- PE_Process case ---------------------------- */
    if($("[name='rd-3']").length > 0){
        $("[name='desc-29'] , [name='desc-30']").attr("disabled",($("[name='rd-3']:checked").val() == 1) ? false : true);
        if($("[name='rd-3']:checked").val() == 1){
            if($("[name='desc-29']").val().length < 1){
                isNotfilled = true;
                if(submit_once) $(`[name="desc-29"]`).addClass("is-invalid");
            }else{
                $(`[name="desc-29"]`).removeClass("is-invalid");
            }
            if($("[name='desc-30']").val().length < 1){
                isNotfilled = true;
                if(submit_once) $(`[name="desc-30"]`).addClass("is-invalid");
            }else{
                $(`[name="desc-30"]`).removeClass("is-invalid");
            }
        }else{
            $("[name='desc-30']").val("");
            $("[name='desc-29']").val("");
            $(`[name="desc-29"] , [name="desc-30"]`).removeClass("is-invalid");
        }
    }
    (!isNotfilled) ? $("#validate-warning").hide() : $("#validate-warning").show();
    return isNotfilled;
}

function checkInputRequired(){
    var isNotfilled = false;

    $("input.required , textarea.required").each(function(i, value) {
        if(this.value == 0){
            isNotfilled = true;
            $(`[name="${this.name}"]`).addClass("is-invalid");
        }else{
            $(`[name="${this.name}"]`).removeClass("is-invalid");
        }
    });

    // $('form#review').find(':submit').attr("disabled",(!isNotfilled) ? false : true);
    (!isNotfilled) ? $("#validate-warning").hide() : $("#validate-warning").show();
    return isNotfilled;
}

function SerializeReviewForm(){
    var f_list = [];
    $("form#review [data-id]:not(:disabled)").each(function (i,v) {
        let rv_id = $(this).data('id');
        if(f_list[`${rv_id}`] == null) f_list[`${rv_id}`] = {id:rv_id, status: null, desc: null}
        
        if($(this).data('type') == "status" && $(this)[0].checked == true){
            f_list[`${rv_id}`].status = this.value;
        }else if($(this).data('type') == "desc"){
            f_list[`${rv_id}`].desc = this.value;
        }
    });
    return f_list.filter(Boolean);
}