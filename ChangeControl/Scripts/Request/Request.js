﻿var submit_Related;
var submit_file;
var remove;
var sendmail;
var GetSummernote;
var test;
var isEditMode;
var TopicStatus = false;
var DepartmentLists;
var InsertRequestPath;
var InsertRelatedPath;
var InsertFilePath;
var file_list = [];
$(document).ready(function () {

    isEditMode = (isEditMode == "True" )? true : false;
    var spanSubmit = $('.pTop');

    spanSubmit.on('click', () => {
        $(this).submit();
    });

    
    $('[data-toggle="datepicker"]').datepicker({
        format: 'dd-MM-yyyy'
    });

    /* -------------------------------------------------------------------------- */
    /*                          Change date configulation                         */
    /* -------------------------------------------------------------------------- */

    $("#change_date_switch").on("click", function (e) {
        $("[name='timing']").datepicker('setDate', (this.checked) ? '01-01-9999' : moment().format("DD-MM-YYYY"));
        $("[name='timing']").prop('disabled', (this.checked) ? true : false);
        if(!this.checked) $("[name='timingDesc']").val(""); 
        $("#change_date_desc").toggle();
    });

    $("[name='timing']").datepicker('setDate', (this.checked) ? '01-01-9999' : Timing);
    if($("[name='timingDesc']").val().length > 0){
        $("#change_date_switch").click();
    }


/* -------------------------------------------------------------------------- */
/*                          Department checkbox list                          */
/* -------------------------------------------------------------------------- */

    $('input[name="changeType"]').change((e) => {
        let val = e.target.value;
        console.log(`changeType val : ${val}`);
        if(val == "Internal"){
            $("#iTopic").removeClass('hidden');
            $("#eTopic").addClass('hidden');
        }else if(val == "External"){
            $("#iTopic").addClass('hidden');
            $("#eTopic").removeClass('hidden');
        }            
        
    })

    $.each(DepartmentLists, (key,val) => {
        if($(`.${val.Name}`).length == $(`.${val.Name}:checked`).length){
            $(`#${val.Name}`).prop('checked', true);
        }
        $(`#${val.Name}`).change(function(e) {
                $(`.${val.Name}`).each(function() {
                    this.checked = (e.target.checked) ? true : false;
                });
        });
    
        $(`.${val.Name}`).click(function () {
            if($(this).is(":checked")) {
                var isAllChecked = 0;
    
                $(`.${val.Name}`).each(function() {
                    if (!this.checked) isAllChecked = 1;
                });
    
                if(isAllChecked == 0) {
                   $(`#${val.Name}`).prop("checked", true);
                }     
            }else{
                $(`#${val.Name}`).prop("checked", false); 
            }
        });
    });
    
/* -------------------------------------------------------------------------- */
/*                               Submit Request                               */
/* -------------------------------------------------------------------------- */

    $("form.Request").submit((e) => {
        e.preventDefault();
        
        // CheckAudit();
        let QC1 = $("input#29").prop("checked");
        let QC2 = $("input#30").prop("checked");
        let QC3 = $("input#31").prop("checked");
            
        let PE1_Process = $("input#32").prop("checked");
        let PE2_Process = $("input#33").prop("checked");
        let P5_ProcessDesign = $("input#44").prop("checked");
        let P6_ProcessDesign = $("input#45").prop("checked");
    
        let isInternal = $("#type_internal").is(":checked");
        let isExternal = $("#type_external").is(":checked");
    
    
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
        
        let form = SerializeReviewForm();
        let quick_form = $(".related_radio").serializeArray();
        for(x in quick_form){
            quick_form[x] = quick_form[x].name;
        }

        files = file_list;
        for(var index in files){
            files[index].file = files[index].detail.file;
            delete files[index].detail;
        }
        $("#loading").removeClass('hidden');
        var inserted_id = "ER-0000000";
        $.post(InsertRelatedPath, { dept_list : quick_form}, () => {
            console.log('Related created');
            $.post(InsertRequestPath, form, (result) => {
                inserted_id = result.id;
                console.log('Topic created');

                if(result.mail != ""){
                    $.post(GenerateMailPath,{ 'mode': result.mail, 'topic_code':inserted_id, 'dept':result.dept, 'pos':result.pos }).fail((error) => {
                        console.error(error);
                        swal("Error", "Cannot send email to Requestor, Please try again", "error");
                        return;
                    })
                }
                
                    var promises = [];
                    console.log('files: ',files);
                    
                    files.forEach(element => {
                        var Data = new FormData();
                        Data.append("file",element.file);
                        Data.append("description",element.description);
                        promises.push($.ajax({
                            type: "POST",
                            url: InsertFilePath,
                            data: Data,
                            cache: false,
                            processData: false,
                            contentType: false,
                            success: function () {
                            },
                            error: function(xhr, status, error) {
                                var errorMessage = xhr.status + ': ' + xhr.statusText
                                alert('Error - ' + errorMessage);
                            }
                        }));
                    });
                    
                    Promise.all(promises).then(() => {
                        $("#loading").addClass('hidden');
                        $("#ReviewSubmit").prop("disabled",true)
                        swal("Success \n NO. " + inserted_id + "", "Complete Transaction", "success").then(setTimeout(() => { RedirectToDetail(inserted_id); }, 1500));;
                    })
                    
                }).fail(() => {
                    $("#loading").addClass('hidden');
                    swal("Error", "Please try again", "error");
                }).catch(e => {
                    console.log(e);
                });


        }).fail((xhr, status, errorThrown) => {
            alert("Insert not success");
                var errorMessage = xhr.status + ': ' + xhr.statusText
                alert('Error - ' + errorMessage);
        })
    });

/* -------------------------------------------------------------------------- */
/*                                Edit Request                                */
/* -------------------------------------------------------------------------- */

    $("form.Edit").submit((e) => {
        e.preventDefault();

        // CheckAudit();
        let QC1 = $("input#29").prop("checked");
        let QC2 = $("input#30").prop("checked");
        let QC3 = $("input#31").prop("checked");
            
        let PE1_Process = $("input#32").prop("checked");
        let PE2_Process = $("input#33").prop("checked");
        let P5_ProcessDesign = $("input#44").prop("checked");
        let P6_ProcessDesign = $("input#45").prop("checked");
    
        let isInternal = $("#type_internal").is(":checked");
        let isExternal = $("#type_external").is(":checked");
    
    
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
        
            let form = SerializeReviewForm();
            let quick_form = $(".related_radio").serializeArray();
            for(x in quick_form){
                quick_form[x] = quick_form[x].name;
            }

            console.log('form: ',form);
            files = file_list;
            for(var index in files){
                files[index].file = files[index].detail.file;
                delete files[index].detail;
            }

            $("#loading").removeClass('hidden');
            var inserted_id = "ER-0000000";
            $.post(InsertRelatedPath,  { dept_list : quick_form}, () => {
                console.log('Related created');
                $.post(UpdateRequestPath, form, (result) =>{
                    console.log('Topic created');
                    inserted_id = result.id;

                    if(result.mail != ""){
                        $.post(GenerateMailPath,{ 'mode': result.mail, 'topic_code':inserted_id, 'dept':result.dept, 'pos':result.pos }).fail((error) => {
                            console.error(error);
                            swal("Error", "Cannot send email to Requestor, Please try again", "error");
                            return;
                        })
                    }
                        var promises = [];
                        
                        files.forEach(element => {
                            if(element.id != null){
                                promises.push($.ajax({
                                    type: "POST",
                                    url: InsertFilePath,
                                    data: Data,
                                    cache: false,
                                    processData: false,
                                    contentType: false,
                                    success: function () {
                                    },
                                    error: function(xhr, status, error) {
                                        var errorMessage = xhr.status + ': ' + xhr.statusText
                                        alert('Error - ' + errorMessage);
                                    }
                                }));
                            }else{
                                var Data = new FormData();
                                Data.append("file",element.file);
                                Data.append("description",element.description);
                                promises.push($.ajax({
                                    type: "POST",
                                    url: InsertFilePath,
                                    data: Data,
                                    cache: false,
                                    processData: false,
                                    contentType: false,
                                    success: function () {
                                    },
                                    error: function(xhr, status, error) {
                                        var errorMessage = xhr.status + ': ' + xhr.statusText
                                        alert('Error - ' + errorMessage);
                                    }
                                    }));
                            }
                        });
                        
                        Promise.all(promises).then(() => {
                            $("#loading").addClass('hidden');
                            $("#ReviewSubmit").prop("disabled",true)
                            swal("Success \n NO. " + inserted_id + "", "Complete Transaction", "success").then(setTimeout(() => { RedirectToDetail(inserted_id); }, 1500));;
                        })
                            
                    }).fail(() => {
                        $("#loading").addClass('hidden');
                        swal("Error", "Please try again", "error");
                    }).catch(e => {
                        console.log(e);
                    });
                }).fail((request, status, error) => {
                    console.error(request.responseText);
                });
    });

/* -------------------------------------------------------------------------- */
/*                              Other Change item                             */
/* -------------------------------------------------------------------------- */

    $("input[name='changeItem'].other,input[name='productType'].other").click((e) => {
        console.log(e);
        if(e.target.checked){
            let path = title = "";
            if(e.target.name == "changeItem"){
                path = InsertOtherChangeItemPath;
                title = "Please enter new Change Item";
            }else if(e.target.name == "productType"){
                path = InsertOtherProductTypePath;
                title = "Please enter new Product type";
            }
            swal({
                title: title, 
                buttons: {
                    cancel: true,
                    confirm: true,
                    closeModal: false,
                },
                content: "input", 
                icon:"warning",
            }).then((res) => {
                if(res != null){
                    if(res.trim().length != 0){
                        
                        $.post(path, {desc:res}, (data) => {
                            if(data){
                                e.target.value = data;
                                e.target.nextElementSibling.textContent = `(${res})`;
                                swal.stopLoading();
                                swal.close();
                            }else{
                                throw err;
                            }
                        },"json").fail(() => {
                            throw err;
                        });
                    }else{
                        swal("Something wrong", "Please enter the text", "error");
                        e.target.nextElementSibling.textContent = null;
                        e.target.checked = false;    
                    }
                }else{
                    e.target.nextElementSibling.textContent = null;
                    e.target.checked = false;
                }
            }).catch(err => {
                e.target.nextElementSibling.textContent = null;
                e.target.checked = false;
                swal("Something wrong", "Please contact admin", "error");
            });
        }
    });



    function RedirectToDetail(id) {
        window.location.replace(`${RedirectDetail}/?id=${id}`);
    }

    function SendMail(type) {
        $.post(sendmail, JSON.stringify({ Type: type,}));
    }
});

function setSubject(subject){
    $(".note-editable:eq(0)").html(subject)
}

function SerializeReviewForm(){
    $("input").prop("disabled",false)
    let form = $("form").serialize();
    return form;
}

function SerializeArrayReviewForm(){
    let form = $("form").serializeArray();
    return form;
}

function CheckAudit(){
    let QC1 = $("input#29").prop("checked");
    let QC2 = $("input#30").prop("checked");
    let QC3 = $("input#31").prop("checked");
        
    let PE1_Process = $("input#32").prop("checked");
    let PE2_Process = $("input#33").prop("checked");
    let P5_ProcessDesign = $("input#44").prop("checked");
    let P6_ProcessDesign = $("input#45").prop("checked");

    let isInternal = $("#type_internal").is(":checked");
    let isExternal = $("#type_external").is(":checked");


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
}

// window.onload = function () {
//         new Vue({
//             //this targets the div id app
//             el: '#vue_change_date',
//             data: {
//             active: true
//         }
//     })
// }