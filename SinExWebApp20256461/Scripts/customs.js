var count = 0;
$(".btn_click").click(function () {
    if ($(this).val() == "Recipient") {
        if (count == 0)
            $("#append").css("display", "block");
        count++;
    }
    if ($(this).val() == "Sender" && count > 0) {
        count--;
        if (count == 0)
            $("#append").css("display", "none");
    }
    console.log(count);
});


$(".btn_sa").click(function () {
    console.log("enter");
    if ($(this).val() == "recipient") {
        $("#recipient").css("display", "block");
        $("#pickup").css("display", "none");
    } else {
        $("#recipient").css("display", "none");
        $("#pickup").css("display", "block");
    }
});