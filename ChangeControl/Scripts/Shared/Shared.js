var pos_select = CreatePositionOption();
pos_select.onchange = function selectChanged(e) {
    value = e.target.value
}

var notyf = new Notyf();

$(() => {
    $("#change_dept_btn").click(() => {
        var dept_select;
        GetDepartmentList().then((departments) => {
            dept_select = CreateDepartmentOption(null,departments);
            dept_select.onchange = function selectChanged(e) {
                value = e.target.value
            }
            swal({
                title: "Developer Mode", 
                text: "Please submit Department", 
                closeOnClickOutside: false,
                content: dept_select,
                buttons : [true,true],
                icon:"warning",
            }).then((res) => {
                if(res){
                    let typedDepartment = $(".select-custom").children("option:selected").text();
                    if(typedDepartment != null){
                        console.log(typedDepartment);
                        $.post(SetDepartmentPath, {DepartmentName:typedDepartment}, (data) => {
                            if(data == 1){
                                swal("Success", "Set Department Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
                            }else{
                                swal("Error", "User Password Not Correct", "error");
                            }
                        },"json");
                    }
                }
            });
        });
    });

    $("[name='remove_file']").click(function(e) {
        swal({
            title: "Delete file", 
            text: "Do you want to delete this file?", 
            closeOnClickOutside: false,
            buttons : [true,true],
            icon:"warning",
        }).then((res) => {
            if(res){
                $.post(DeleteFileByNameFormatPath, {name_format : this.value.split("^")[0]}, (res) => {
                    if(res.status == 'success'){
                        swal("Success", "Remove file success", "success").then(setTimeout(() => { location.reload(); }, 1500));
                    }else{
                        swal("Error", "Remove file not success, Please contact admin", "error");
                    }
                });
            }
        });
    });

})

function CreateDepartmentOption(dept = null,departments){
    var value;
    const select = document.createElement('select');
    select.className = 'select-custom'
    let i=1;
    departments.forEach(element => {
        let option = document.createElement('option');
        if(dept != null) option.selected = (element == dept) ? true : false;
        option.innerHTML = element;
        option.value = i;
        select.appendChild(option);
        i++;
    })
    return select;
}

function SetPosition(){
    swal({
        title: "Developer Mode", 
        text: "Please enter password", 
        closeOnClickOutside: false,
        buttons : [true,true],
        content: {
            element: "input",
            attributes: {
                placeholder: "Type your password",
                type: "password",
            },
        },icon:"warning",
    }).then((password) => {
        if(password != null){
            if(password == "wmpobxxvoFfg,o!@#$"){
                swal({
                    title: "Developer Mode", 
                    text: "Please select Position", 
                    closeOnClickOutside: false,
                    buttons : [true,true],
                    content: pos_select,
                    icon:"warning",
                }).then(() => {
                    let typedPosition = $(".select-custom").children("option:selected").text();
                    console.log(typedPosition);
                    $.post(SetPositionPath, {PositionName:typedPosition}, (data) => {
                        if(data == 1){
                            swal("Success", "Set Position Complete", "success").then(location.reload());
                        }else{
                            swal("Error", "User Password Not Correct", "error");
                        }
                    },"json");
                });
            }else{
                swal("Error", "User Password Not Correct", "error");
            }
        }
    });
}

function CreatePositionOption(){
    var value;
    var position = [ "Issue","Approver","Admin"];

    const select = document.createElement('select');
    select.className = 'select-custom'
    let i=1;
    position.forEach(element => {
        let option = document.createElement('option');
        option.innerHTML = element;
        option.value = i;
        select.appendChild(option);
        i++;
    });
    return select;
}

async function GetDepartmentList(user = null){
    if(user == null){
        await $.post(GetDepartmentListPath,(result) => {
            departments = result.data;
        });
    }else{
        await $.post(GetDepartmentListByUserIDPath, {us_id:user} ,(result) => {
            if(result != null){
                departments = result.data;
            }
        });
    }

        return departments;
}

function GetSession(){
    $.post(GetSessionPath,(res) => console.log(res));
}



function NoPermissionAlert(){
    swal({
        title: "คุณไม่มีสิทธิ์การเข้าถึง", 
        text: "ขออภัย คุณไม่มีสิทธิ์การเข้าถึงข้อมูลดังกล่าว หากเกิดข้อผิดพลาดกรุณาติดต่อฝ่าย IT เบอร์ 2064", 
        icon:"error",
    })
}

$(() => {
    var NoPermissionDetected = false;
    if(NoPermissionDetected) NoPermissionAlert();
    NoPermissionAlert = false;
})
