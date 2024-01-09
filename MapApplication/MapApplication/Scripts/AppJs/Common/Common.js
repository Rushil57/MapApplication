var progressInterval = null;
let progress = 0;
let timeoutSec = 100;
var isViewProgressBar = false;
var progressTillUpdateEle = $('#progressTillUpdate');
var progressTillUpdateValueEle = $('#progressTillUpdateValue');
var progressBarEle = $('.progressBar');
function AddLoader() {
    $('.overlay').removeClass('d-none');
    progressBarEle.addClass('d-none');
    SetProgressToZero();
    if (isViewProgressBar) {
        progressBarEle.removeClass('d-none');
        progressInterval = setInterval(
            (function () {
                console.log("Progress: " + progress + "/" + timeoutSec)
                document.getElementById("progressTillUpdate").max = timeoutSec;
                document.getElementById("progressTillUpdate").value = progress; // Does not update for some reason
                progressTillUpdateValueEle.text(progress + " %")
                //if (progress == timeoutSec) { RemoveLoader() }
                //progress = (progress < timeoutSec) ? progress + 1 : 0; // Loop after timeoutSec 
                ReadProgressCount();
            }), 2 * 1000);
    }
}
function SetProgressToZero() {
    progress = 0;
    timeoutSec = 100;
    progressTillUpdateValueEle.text(progress + " %")
    document.getElementById("progressTillUpdate").max = timeoutSec;
    document.getElementById("progressTillUpdate").value = progress;
}
function ReadProgressCount() {
    $.ajax({
        url: "/Home/ReadProgressCount",
        type: "GET",
        processData: false,
        contentType: false,
        async: false,
        success: function (data) {
            let currData = JSON.parse(data);
            let currProgress = currData.data;
            let currProgressArr = currProgress.split("/");

            progress = currProgress.trim() != "0/0" ? (Number(currProgressArr[0]) / Number(currProgressArr[1]) * 100).toFixed(0) : progress;
        },
        error: function (err1, err2, err3) {
        }
    });

}
function RemoveLoader() {

    if (progressInterval != null) {
        while (Number(progress) < timeoutSec) {

            var tempProgressNo = Number(progress) > 100 ? 100 : Number(progress);
            document.getElementById("progressTillUpdate").value = tempProgressNo;
            progressTillUpdateValueEle.text(tempProgressNo + " %")
            progress = tempProgressNo + 5;
        }
        document.getElementById("progressTillUpdate").value = timeoutSec;
        setTimeout(function () { clearInterval(progressInterval); SetProgressToZero() }, 1000)

    }
    setTimeout(function () { $('.overlay').addClass('d-none'); }, 1000)
}

