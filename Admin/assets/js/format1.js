/*
 * 消息管理
 * 活動管理
*/

// 被刪除的圖片
var delImgs = [];

// 重置新增消息
$(".btnExt, .btnEdit").click(() => {
    $(".btnReset").click();
    document.getElementById("fmEditItem").reset();
    delImgs = [];
});

// 置頂切換
$(".btnTopSwitch").change((e) => {
    var top = e.target.checked ? "Y" : "N";
    var id = e.target.dataset.sn;

    var postData = new FormData();
    postData.append("top", top);

    var xhr = new XMLHttpRequest();
    xhr.open("POST", topu + "/" + id, true);
    xhr.send(postData);
});

function Save(e) {

    var requiredCheck = true;

    $("#fmEditItem").find("input[required], textarea[required]").each((idx, obj) => {
        if (obj.value.length === 0) {
            Swal.fire('請完整填寫所有必填項！');
            requiredCheck = false;
        }
    });
    $("#fmEditItem").find("input, textarea").each((idx, obj) => {
        if ($(obj).attr("maxlength") !== undefined && obj.value.length >= $(obj).attr("maxlength")) {
            Swal.fire($("label[for=" + obj.id + "]").text() + '已超過字數限制！');
            requiredCheck = false;
        }
    });

    if (!requiredCheck)
        return;

    var id = e.target.dataset.sn;
    $("#hidDelIds").val(JSON.stringify(delImgs));
    var postData = new FormData($("#fmEditItem")[0]);
    var xhr = new XMLHttpRequest();
    xhr.open("POST", edtu + "/" + id, true);
    xhr.onload = function () {
        if (this.status === 200 && this.responseText === "OK") {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: '操作成功',
                showConfirmButton: false,
                timer: 1000
            }).then((result) => {
                window.location.href = window.location.href;
            });
        }
    };
    xhr.send(postData);
}

// 刪除
function Delete(e) {
    Swal.fire({
        icon: 'warning',
        title: '確定要刪除嗎?',
        text: '刪除後不可複原，請再次確認是否要刪除？',
        showConfirmButton: true,
        showCancelButton: true,
        confirmButtonText: '確定',
        cancelButtonText: '取消'
    }).then((result) => {
        if (result.isConfirmed) {
            var id = e.target.dataset.sn;

            var xhr = new XMLHttpRequest();
            xhr.open("POST", delu + "/" + id, true);
            xhr.onload = function () {
                if (this.status === 200 && this.responseText === "OK") {
                    Swal.fire({
                        position: 'top-end',
                        icon: 'success',
                        title: '操作成功',
                        showConfirmButton: false,
                        timer: 1000
                    });

                    e.target.parentElement.parentElement.parentElement.remove(e.target.parentElement.parentElement);
                }
                else {
                    Swal.fire({
                        position: 'top-end',
                        icon: 'error',
                        title: '操作失敗',
                        showConfirmButton: false,
                        timer: 1000
                    });
                }
            };
            xhr.send();
        }
    });
}