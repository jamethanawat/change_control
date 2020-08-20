var loader;

$(document).ready(function () {
    loader = new ldLoader({ root: document.getElementById("signin-btn") });
    document.getElementById("user").addEventListener("keyup", function(event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            document.getElementById("pass").focus();
        }
    });

    document.getElementById("pass").addEventListener("keyup", function(event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            $("#signin-btn").click();
        }
    });

    $("#signin-btn").click(function (event) {
        event.preventDefault();

        var Iuser = $('#user').val();
        var Ipass = $('#pass').val();

        // var select = CreateDepartmentOption();
        // select.onchange = function selectChanged(e) {
        //     value = e.target.value
        // }
        

        console.log(Iuser.length);
        console.log(Ipass.length);

        if (Iuser.length == 0 || Ipass.length == 0) {
            swal("Error", "User Password Not Correct", "error");
        }else{
            ajaxUser(Iuser, Ipass);
            function ajaxUser(Iuser, Ipass){
                loader.toggle();
                $.post(CheckUserPath,({ username:Iuser,password:Ipass }) ,(res) => {
                    var promises = [];
                    if(res.pos == "Special"){
                        var select = CreateDepartmentOption(null,Iuser)
                        select.onchange = function selectChanged(e) { value = e.target.value }
                        loader.toggle();
                        
                        swal({
                                title: "Confirm Department", 
                                text: "Please select your Department",
                                closeOnClickOutside: false,
                                // buttons : [true,true],
                                content: select,
                            icon:"warning",
                        }).then(() => {
                            loader.toggle();
                            let selected_dept = $(".select-custom").children("option:selected").text();
                            $.post(SetDepartmentAltPath,{dept:selected_dept},(success) => {
                                if(success){
                                    swal("Success", "Sign in complete", "success").then( window.location.href = (UrlTopicID.length > 8 ) ? NavigateToPrevious : NavigateToHome );
                                }
                            });
                        });
                    }else if(res.status == "success"){
                        swal("Success", "Sign in complete", "success").then( window.location.href = (UrlTopicID.length > 8 ) ? NavigateToPrevious : NavigateToHome );
                    }else if(res.status == "missdept"){
                        var select = CreateDepartmentOption(res.data);
                        select.onchange = function selectChanged(e) { value = e.target.value }
                        loader.toggle();

                        swal({
                            title: "Confirm Department", 
                            text: "Please select your Department",
                            closeOnClickOutside: false,
                            // buttons : [true,true],
                            content: select,
                            icon:"warning",
                        }).then(() => {
                            loader.toggle();
                            let selected_dept = $(".select-custom").children("option:selected").text();
                            $.post(SetDepartmentAltPath,{dept:selected_dept},(success) => {
                                if(success){
                                        swal("Success", "Sign in complete", "success").then( window.location.href = (UrlTopicID.length > 8 ) ? NavigateToPrevious : NavigateToHome );
                                }
                            });
                        });
                    }else{
                        loader.toggle();

                        if(res.status == "wrong_us"){
                            swal("Error", "Username is wrong.", "error");
                        }else if(res.status == "wrong_pwd"){
                            swal("Error", "Password is wrong.", "error");
                        }else if(res.status == "error"){
                            swal("Error", "Something is wrong, Please contact admin. ", "error");
                        }
                    }
                });
                // swal("Error", "Cannot Not Connect Database", "error");
            }
            
            function ajaxAdmin(Iuser, Ipass, Idepartment){
                $.ajax({
                    type: "POST",
                    url: CheckAdminPath,
                    data: JSON.stringify({
                        username: Iuser,
                        password: Ipass,
                        department: Idepartment,
                    }),
                    contentType: 'application/json; charset=utf-8',
                    success: function (response) {           
                        if (response != "Error") {
                            console.log("gotoHome");
                            window.location.href = response.Url;
                        }else{
                            swal("Error", "User Password Not Correct", "error");
                        }
                    },
                    error: function () {
                        swal("Error", "Cannot Not Connect Database", "error");
                    }
                });
            }

               

        }

    });


});
