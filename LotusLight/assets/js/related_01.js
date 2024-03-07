
// 捐款
$(".btnDonate").click((e) => {

    e.preventDefault();

    var buyerName = $(".relatedTitle").val().trim();
    var payType = $(".donateMethods").val().trim();
    var tel = $(".relatedTel").val().trim();
    var amount = $(".totals").text();
    var city = $(".addReceiptCity option:selected");
    var area = $(".addReceiptTown option:selected");
    var pickupMethod = $(".relatedPicks option:selected");
    var sumQty = 0;

    $(".quantities").each((idx, obj) => { sumQty += parseInt(obj.value); });

    if (buyerName.length === 0) {
        Swal.fire('請填寫 捐款姓名（公司）！');
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

    if (pickupMethod.val() == 0) {
        Swal.fire('請選擇 取貨方式！');
        return;
    }

    if (sumQty <= 0) {
        Swal.fire('請填寫 數量！');
        return;
    }

    var remark = $(".remark").val().trim();

    if (remark.length === 0) {
        Swal.fire('請填寫 地址或備註！');
        return;
    } else if (remark.length > 249) {
        Swal.fire('地址或備註太長了！');
        return;
    }

    if (city.index() > 0)
        remark = city.text() + area.text() + remark;

    var buyerUnit = $(".relatedUnit[name=relatedUnit]").val();
    var buyerId = $(".relatedIdNum").val().trim();
    var buyerPhone = $(".relatedTel").val().trim();
    var buyerEmail = $(".relatedMail").val().trim();
    var donateUnit = $(".relatedBranch option:selected").val();
    var products = "";
    
    if (donateUnit === 'reset') {
        Swal.fire('請選擇贈予單位！');
        return;
    }
    $(".quantities").each((idx, obj) => {
        if (obj.value > 0) {
            products += `${obj.dataset.name}=${obj.value};`;
        }
    });

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
            postData.append("OrderTypeId", "2");
            postData.append("BuyerTypeId", buyerUnit);
            postData.append("BuyerName", buyerName);
            postData.append("BuyerId", buyerId);
            postData.append("BuyerPhone", buyerPhone);
            postData.append("BuyerEmail", buyerEmail);
            postData.append("PayType", payType);
            postData.append("DonateUnit", donateUnit);
            postData.append("DonateAmount", amount);

            postData.append("Products", products);
            postData.append("PickupMethod", pickupMethod.text());
            postData.append("Remark", remark);


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
                        var m = $("#relatedBranch option:selected").data("m-code");
                        var t = $("#relatedBranch option:selected").data("t-code");
                        var n = $("#relatedBranch option:selected").text();

                        var form = $(".fmDonate");
                        form.find("#OrderNo").val(orderId);
                        form.find("#MerchantId").val(m);
                        form.find("#TerminalId").val(t);
                        form.find("#MerchantName").val(n);
                        form.find("#PayType").val(payType);
                        form.find("#Amount").val(amount);
                        form.find("#Mobile").val(buyerPhone);
                        form.find("#CardHolder").val(buyerName);
                        form.find("#Product").val(products);
                        form.find("#Email").val(buyerEmail);
                        form.find("#ReceiveUrl").val(`${window.origin}/Home/Receive`);
                        form.find("#ReturnUrl").val(`${window.origin}/Home/Return`);

                        window.open('', 'new_win');

                        form.submit();
                    }
                } else {

                    Swal.fire('操作失敗！');

                }
            };
            xhr.send(postData);

        }

    });
});