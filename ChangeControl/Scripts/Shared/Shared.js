
var password = "63014@tnsadmin";
var DevMode = "on";
var department_list = CreateDepartmentOption();
var position_list = CreatePositionOption();
department_list.onchange = function selectChanged(e) {
    value = e.target.value
}
position_list.onchange = function selectChanged(e) {
    value = e.target.value
}

function SetDepartment(){
    swal({
        title: "Developer Mode", 
        text: "Please enter password", 
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
            if(password == this.password || DevMode == "on"){
                swal({
                    title: "Developer Mode", 
                    text: "Please submit Department", 
                    content: department_list,
                    buttons : [true,true],
                    icon:"warning",
                }).then((res) => {
                    if(res){
                        let typedDepartment = $(".select-custom").children("option:selected").text();
                        if(typedDepartment != null){
                            console.log(typedDepartment);
                            $.post("/login/SetDepartment", {DepartmentName:typedDepartment}, (data) => {
                                if(data == 1){
                                    swal("Success", "Set Department Complete", "success").then(location.reload());
                                }else{
                                    swal("Error", "User Password Not Correct", "error");
                                }
                            },"json");
                        }
                    }
                });
            }else{
                swal("Error", "User Password Not Correct", "error");
            }
        }
    });
}

function CreateDepartmentOption(){
    var value;
    var position = [ "PE1_Process","PE2_Process","MKT","IT","PE","PCH","PT","PC1","PC2","QC1","QC2","QC3","QC_IN","QC_NFM","QC_FINAL"];

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

function SetPosition(){
    swal({
        title: "Developer Mode", 
        text: "Please enter password", 
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
            if(password == this.password || DevMode == "on"){
                swal({
                    title: "Developer Mode", 
                    text: "Please select Position", 
                    content: position_list,
                    icon:"warning",
                }).then(() => {
                    let typedPosition = $(".select-custom").children("option:selected").text();
                    console.log(typedPosition);
                    $.post("/login/SetPosition", {PositionName:typedPosition}, (data) => {
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
    var position = [ "Staff","Manager"];

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
