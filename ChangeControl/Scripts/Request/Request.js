var submit_Related;
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
    

    $("form.Request").submit((e) => {
        e.preventDefault();
        
        let QC1 = $("input#29").prop("checked");
        let QC2 = $("input#30").prop("checked");
        let QC3 = $("input#31").prop("checked");
            
        let PE1_Process = $("input#32").prop("checked");
        let PE2_Process = $("input#33").prop("checked");

        let isInternal = $("#type_internal").is(":checked");
        let isExternal = $("#type_external").is(":checked");


        if(isInternal){
            if(!(PE1_Process || PE2_Process) || !(QC1 || QC2 || QC3)){ //Need to select PE_Process or QC as Auditor at lease one
                swal("Warning", "Please select PE_Process and QC at least one", "warning");
                return;
            }else if(Number(QC1) + Number(QC2) + Number(QC3) != 1 && ((PE1_Process ^ PE2_Process)) == false){ //When select QC and PE_Process more than one
                swal("Warning", "Please select one QC and one PE_Process", "warning");
                return
            }else if(Number(QC1) + Number(QC2) + Number(QC3) != 1 ){ //When select QC more than one
                swal("Warning", "Please select one QC", "warning");
                return
            }else if(((PE1_Process ^ PE2_Process)) == false){ //When select PE_Process more than one
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

        files = file_list;
        for(var index in files){
            files[index].file = files[index].detail.file;
            delete files[index].detail;
        }
        $("#loading").removeClass('hidden');
        var inserted_id = "ER-0000000";
        $.post(InsertRelatedPath, form, () => {
            console.log('Related created');
            $.post(InsertRequestPath, form, (id) => {
                console.log('Topic created');
                inserted_id = id;
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
                    promises.push(
                        $.post(GeneratePath,{
                            'mode':'EmailRequestor',
                            'topic_code':inserted_id,
                        }).fail((error) => {
                            console.err(error);
                            swal("Error", "Cannot send email to Requestor, Please try again", "error");
                            return;
                        })
                    );
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

    $("form.Edit").submit((e) => {
        e.preventDefault();
            let form = SerializeReviewForm();
            let topic_id = $("form.Edit").attr('id');
            console.log('form: ',form);
            files = file_list;
            for(var index in files){
                files[index].file = files[index].detail.file;
                delete files[index].detail;
            }

            $("#loading").removeClass('hidden');
            var inserted_id = "ER-0000000";
            $.post(InsertRelatedPath,form, () => {
                console.log('Related created');
                $.post(UpdateRequestPath, form, (id) =>{
                    console.log('Topic created');
                    inserted_id = id;
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

    function RedirectToDetail(id) {
        window.location.replace(`${RedirectDetail}/?id=${id}`);
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


        swal("Success \n Change Control NO. " + code + "", "Complete Transaction", "success");
    });

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

