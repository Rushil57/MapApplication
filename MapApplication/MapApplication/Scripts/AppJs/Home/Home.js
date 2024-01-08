isViewProgressBar = true;
function Submit() {
    AddLoader();
    var pdata = new FormData();
    var files = $("#certImageFile").get(0).files;
    pdata.append('certImageFile', files[0]);
    $.ajax({
        url: "Home/Submit",
        type: "POST",
        data: pdata,
        processData: false,
        contentType: false,
        success: function (data) {
            var response = JSON.parse(data);
            alert(response.data);
            var input = $("#certImageFile");
            input.replaceWith(input.val('').clone(true));
            RemoveLoader();
        },
        error: function (err1, err2, err3) {
            RemoveLoader();
        }
    });

}