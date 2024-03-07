// 觸發收據change事件
$(document).ready(() => {
    $(".receipt:checked").change();
});

// 金額防呆
$(".donateAmount").change((e) => {
    var payType = $(".donateMethods option:selected").val();
    var amount = e.target.value;

    if (amount < 100)
        amount = 100;

    // 超過 20000 不能選超商
    if (payType > 3 && payType < 11 && amount > 20000) {
        $(".donateAmount").val(20000);
    }
    else if (amount > 20000) {
        $(".donateMethods optgroup").attr("disabled", "disabled");
    } else {
        $(".donateMethods optgroup").removeAttr("disabled");
    }
});

// 超商不能超過 20000
$(".donateMethods").change((e) => {
    var payType = $(".donateMethods option:selected").val();
    var amount = $(".donateAmount").val();

    if (payType > 3 && payType < 11) {
        if (amount > 20000)
            $(".donateAmount").val(20000);

        $(".donateAmount").attr("max", 20000);
    } else {
        $(".donateAmount").removeAttr("max");
    }
});

// 其他捐款用途
$(".donatePurpose").change((e) => {
    var purpose = $(".donatePurpose option:selected").val();
    if (purpose == 15) {
        $(".otherPurposeBlock").show();
    }
    else {
        $(".otherPurposeBlock").hide();
    }
});

// 捐款
$(".btnSave").click((e) => {

    var buyerName = $(".donateTitle").val().trim();
    var addr = $(".receiptAddress").val().trim();
    var payType = $(".donateMethods").val().trim();
    var tel = $(".donateTel").val().trim();
    var amount = $(".donateAmount").val().trim();
    var city = $(".addReceiptCity option:selected");
    var area = $(".addReceiptTown option:selected");
    var purpose = $(".donatePurpose option:selected");
    var otherPurpose = $(".donateOtherPurpose").val().trim();
    var donatePurpose = purpose.text();

    if (purpose.val() == 15)
        donatePurpose = "其他：" + otherPurpose;

    if (buyerName.length === 0) {
        Swal.fire('請填寫 捐款姓名（公司）！');
        return;
    }

    if (addr.length === 0) {
        Swal.fire('請填寫 地址！');
        return;
    } else if (addr.length > 249) {
        Swal.fire('地址太長了！');
        return;
    }

    if (payType > 3 && payType < 11 && tel.length === 0) {
        Swal.fire('選擇超商代碼時，請填寫 聯絡電話（手機號碼）！');
        return;
    }

    if (payType == 1 && tel.length === 0) {
        Swal.fire('選擇線上刷卡時，請填寫 聯絡電話（手機號碼或市話）！');
        return;
    }

    if (purpose.val() == 15 && otherPurpose.length === 0) {
        Swal.fire('選擇其他時，請填寫 其他捐款用途！');
        return;
    }

    if (amount.length === 0) {
        Swal.fire('請填寫 捐款金額！');
        return;
    }

    if (city.index() > 0)
        addr = city.text() + area.text() + addr;

    var buyerUnit = $(".donateUnit[name=donateUnit]").val();
    var buyerId = $(".donateIdNum").val().trim();
    var buyerPhone = $(".donateTel").val().trim();
    var buyerEmail = $(".donateMail").val().trim();
    var donateUnit = $(".donateBranch option:selected").val();
    var receiptTitle = $(".receiptTitle").length === 0 ? "" : $(".receiptTitle").val().trim();

    if (donateUnit === 'reset') {
        Swal.fire('請選擇贈予單位！');
        return;
    }
    var needReceipt = $(".receipt[name=receipt]:checked").val();
    var receiptPostMethod = needReceipt == "Y" ? $(`label[for=${$("input[name=deliveryMethod]:checked")[0].id}]`).text() : "";
    var needAnonymous = $(".public[name=public]:checked").val();

    Swal.fire({
        title: '感謝您的善心',
        html: `<p style='font-weight:800'>本次捐款金額為 : ${amount} 元</p><br><p>請確認金額</p>`,
        showConfirmButton: true,
        showCancelButton: true,
        confirmButtonText: '確定送出',
        cancelButtonText: '取消'
    }).then((result) => {

        if (result.isConfirmed) {

            // 建立訂單
            var postData = new FormData();
            postData.append("OrderTypeId", "1");
            postData.append("BuyerTypeId", buyerUnit);
            postData.append("BuyerName", buyerName);
            postData.append("BuyerId", buyerId);
            postData.append("BuyerPhone", buyerPhone);
            postData.append("BuyerEmail", buyerEmail);
            postData.append("PayType", payType);
            postData.append("DonateUnit", donateUnit);
            postData.append("DonateAmount", amount);
            postData.append("DonateFor", donatePurpose);
            postData.append("NeedReceipt", needReceipt);
            postData.append("ReceiptPostMethod", receiptPostMethod);
            postData.append("ReceiptTitle", receiptTitle);
            postData.append("ReceiptAddress", addr);
            postData.append("NeedAnonymous", needAnonymous);

            var xhr = new XMLHttpRequest();
            xhr.open("POST", window.location.origin + "/Act/GoDonate", true);
            xhr.onload = function () {
                if (this.status === 200 && this.responseText.indexOf("OK") > -1) {
                    var orderId = this.responseText.split(";")[1];

                    if (payType > 80) {
                        Swal.fire({ title: '感謝您的善心！', text: '本選項僅協助捐款人紀錄聯絡方式，並非線上繳款，請自行利用ATM、EATM、網路銀行方式轉帳善款至各分會的實體戶頭。' });
                    }
                    else {
                        // 送金流
                        var m = $("#donateBranch option:selected").data("m-code");
                        var t = $("#donateBranch option:selected").data("t-code");
                        var n = $("#donateBranch option:selected").text();

                        var form = $(".fmDonate");
                        form.find("#OrderNo").val(orderId);
                        form.find("#MerchantId").val(m);
                        form.find("#TerminalId").val(t);
                        form.find("#MerchantName").val(n);
                        form.find("#PayType").val(payType);
                        form.find("#Amount").val(amount);
                        form.find("#Mobile").val(buyerPhone);
                        form.find("#CardHolder").val(buyerName);
                        form.find("#Product").val(donatePurpose);
                        form.find("#Email").val(buyerEmail);
                        form.find("#ReceiveUrl").val(`${window.origin}/Home/Receive`);
                        form.find("#ReturnUrl").val(`${window.origin}/Home/Return`);

                        form.submit();

                        // window.open('', 'new_win');
                    }
                } else {

                    Swal.fire('操作失敗！');

                }
            };
            xhr.send(postData);

        }

    });

});