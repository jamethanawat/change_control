var Report;
$(document).ready(function () {

    $('#date_range_report').daterangepicker({
        locale: {
            format: 'DD/MM/YYYY'
        }
    })
    $('#date_range_report').data('daterangepicker').setStartDate(moment().format('DD/MM/YYYY'));
    $('#date_range_report').data('daterangepicker').setEndDate(moment().format('DD/MM/YYYY'));


    $("#submit").click((e) => {
        e.preventDefault();
        $("#loading").removeClass('hidden');     
        var StartDate = ($("#date_range_report").is(":disabled")) ? null : $('#date_range_report').data('daterangepicker').startDate.format('YYYYMMDD');
        var EndDate = ($("#date_range_report").is(":disabled")) ? null : $('#date_range_report').data('daterangepicker').endDate.format('YYYYMMDD');
        var request = new XMLHttpRequest();
        request.responseType = "blob";
        request.open("GET", Report.concat("?StartDate=" + StartDate + "&EndDate=" + EndDate+""));
        request.onload = function () {
            if (this.response.size <= 11) {
                $('#loading').addClass('hidden');
                swal({
                    title: "ERROR",
                    text: "Data not Found",
                    type: "warning",
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: "OK"
                })
            } else {
                $('#loading').addClass('hidden');   
                var url = window.URL.createObjectURL(this.response);
                var a = document.createElement("a");
                document.body.appendChild(a);
                a.href = url;
                a.download = "Summary Data"
                //a.download = this.response.name || "download Report" + $.now()
                a.click();
            }
        }
        request.send();
    });
});
