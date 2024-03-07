$(document).ready(() => {
    $(".receipt:checked").change();
});


$(".donateAmount").change((e) => {
    if (e.target.value < 100)
        e.target.value = 100;
});

$(".donateMethods").change((e) => {
    if (parseInt($("#donateMethods").val()) > 3
        && parseInt($("#donateMethods").val()) < 11) {
        if (parseInt($(".donateAmount").val()) > 20000)
            $(".donateAmount").val(20000);

        $(".donateAmount").attr("max", 20000);
    } else {
        $(".donateAmount").removeAttr("max");
    }
});

$(".btnSave").click((e) => {
    if ($(".donateTitle").val().length === 0) {
        alert('請填寫 捐款姓名（公司）！');
        return;
    }
    if ($(".receiptAddress").val().length === 0) {
        alert('請填寫 地址！');
        return;
    }
    if (parseInt($("#donateMethods").val()) > 3
        && parseInt($("#donateMethods").val()) < 11
        && $(".donateTel").val().length === 0) {
        alert('選擇超商代碼時，請填寫 聯絡電話（手機號碼）！');
        return;
    }
    if (parseInt($("#donateMethods").val()) === 1
        && $(".donateTel").val().length === 0) {
        alert('選擇線上刷卡時，請填寫 聯絡電話（手機號碼或市話）！');
        return;
    }
    if ($(".donateAmount").val().length === 0) {
        alert('請填寫 捐款金額！');
        return;
    }


    var d = new Date();
    var i = String(d.getFullYear()) + String(d.getUTCMonth()) + String(d.getUTCDate()) + String(d.getUTCHours()) + String(d.getUTCMinutes()) + String(d.getUTCMilliseconds());
    var m = $("#donateBranch option:selected").data("m-code");
    var t = $("#donateBranch option:selected").data("t-code");
    var n = $("#donateBranch option:selected").text();
    var o = $("#donateMethods option:selected").val();
    var a = $(".donateAmount").val();
    var p = $("#donateTel").val();
    var c = $(".donateTitle").val();
    var pn = $(".donatePurpose option:selected").text();
    var em = $(".donateMail").val();
    var addr = $(".addReceiptCity option:selected").val() + $(".addReceiptTown option:selected").text() + $(".receiptAddress").val();
    // var 

    if (addr.length >= 200) {
        alert('地址或備註不得超過200字！');
        return;
    }

    var form = $(".fmDonate");
    form.find("#OrderNo").val(i);
    form.find("#MerchantId").val(m);
    form.find("#TerminalId").val(t);
    form.find("#MerchantName").val(n);
    form.find("#PayType").val(o);
    form.find("#Amount").val(a);
    form.find("#Mobile").val(p);
    form.find("#CardHolder").val(c);
    form.find("#Product").val(pn);
    form.find("#Email").val(em);
    form.find("#OrderDesc").val(addr);

    window.open('', 'new_win');

    form.submit();

    //var postData = new FormData();
    //postData.append('OrderNo', i);
    //postData.append('MerchantId', m);
    //postData.append('TerminalId', t);
    //postData.append('Encoding', 'utf-8');
    //postData.append('MerchantName', n);
    //postData.append('PayType', o);
    //postData.append('Amount', a);
    //postData.append('Mobile', p);
    //postData.append('CardHolder', c);
    //postData.append('Product', pn);
    //postData.append('Email', em);
    //postData.append('OrderDesc', addr);
    //var xhr = new XMLHttpRequest();
    //xhr.open('POST', 'https://service.payware.com.tw/wpss/authpay.aspx');
    /*xhr.onreadystatechange = function () {
        if (this.readyState === 4) {
            if (this.status === 200) {
                var ajax_response_text = req.responseText;
            }
        }
    };*/
    //xhr.send(postData);

    //var u = `https://service.payware.com.tw/wpss/authpay.aspx?OrderNo=${i}&MerchantId=${m}&TerminalId=${t}&Encoding=utf-8&MerchantName=${n}&PayType=${o}&Amount=${a}&Mobile=${p}&CardHolder=${c}&Product=${pn}&Email=${em}`;
    //window.open(u, '_blank');

    // 建立訂單
    /*var postData = new FormData();
    var xhr = new XMLHttpRequest();

    postData.append("amt", $(".donateAmount").val());
    postData.append("opt", $(".donatePurpose:selected").val());

    xhr.open("POST", window.location.origin + "/Home/GoDonate", true);

    // 建成功在送金流
    xhr.onload = function () {
        if (this.status === 200) {
            if (parseInt(o) < 80) {
                i = this.responseText; // 如果有取到訂單號就直接取代隨機單號
                var u = `https://service.payware.com.tw/wpss/authpay.aspx?OrderNo=${i}&MerchantId=${m}&TerminalId=${t}&Encoding=utf-8&MerchantName=${n}&PayType=${o}&Amount=${a}&Mobile=${p}&CardHolder=${c}&Product=${pn}&Email=${em}`;
                window.open(u, '_blank');
            }
        }
    };

    xhr.send(postData);*/
});

/*$(".btnDonate").click((e) => {
    if ($(".relatedTitle").val().length === 0) {
        alert('請填寫 捐款姓名（公司）！');
        return;
    }
    if (parseInt($("#relatedMethods:selected").val()) > 3 && $(".relatedTel").val().length === 0) {
        alert('選擇超商代碼時，請填寫 聯絡電話（手機號碼）！');
        return;
    }
    if ($(".totals").text() === "0") {
        alert('總計等於0！');
        return;
    }

    var pm = $(".relatedPicks").val();

    if (pm === "0") {
        alert('請選擇 取貨方式！');
        return;
    }

    var i1 = $(".quantities")[0].value;
    var i2 = $(".quantities")[1].value;
    var i3 = $(".quantities")[2].value;

    var item = "";
    if (i1.length > 0 && parseInt(i1) > 0)
        item = item + "蓮生活佛水瓶*" + i1 + ";";
    if (i2.length > 0 && parseInt(i2) > 0)
        item = item + "貴人多助筆或招財進寶筆*" + i2 + ";";
    if (i3.length > 0 && parseInt(i3) > 0)
        item = item + "開運印章*" + i3 + ";";

    if (pm === "1") {
        item = item + $(".addReceiptCity option:selected").val() + $(".addReceiptTown option:selected").text() + $(".shippingAddress").val() + ";";

        if ($(".shippingAddress").val().length === 0) {
            alert('請填寫 運送地址！');
            return;
        }
    }
    if (pm === "2") {
        item = item + $(".storeAddress").val() + ";";

        if ($(".storeAddress").val().length === 0) {
            alert('請填寫 運送地址！');
            return;
        }
    }
    if (pm === "4" || pm === "3") {
        item = item + $(".shippingNote").val() + ";";

        if ($(".shippingNote").val().length === 0) {
            alert('請填寫 備註！');
            return;
        }
    }
    if (item.length >= 200) {
        alert('地址或備註不得超過200字！');
        return;
    }

    var d = new Date();
    var i = String(d.getFullYear()) + String(d.getUTCMonth()) + String(d.getUTCDate()) + String(d.getUTCHours()) + String(d.getUTCMinutes()) + String(d.getUTCMilliseconds());
    var m = $("#relatedBranch option:selected").data("m-code");
    var t = $("#relatedBranch option:selected").data("t-code");
    var n = $("#relatedBranch option:selected").text();
    var o = $("#donateMethods option:selected").val();
    var a = $(".totals").text();
    var p = $(".relatedTel").val();
    var c = $(".relatedTitle").val();

    var pn = "";
    var em = $(".relatedMail").val();

    var form = $(".fmDonate");
    form.find("#OrderNo").val(i);
    form.find("#MerchantId").val(m);
    form.find("#TerminalId").val(t);
    form.find("#MerchantName").val(n);
    form.find("#PayType").val(o);
    form.find("#Amount").val(a);
    form.find("#Mobile").val(p);
    form.find("#CardHolder").val(c);
    form.find("#Product").val(pn);
    form.find("#Email").val(em);
    form.find("#OrderDesc").val(item);

    window.open('', 'new_win');

    form.submit();

    //var postData = new FormData();
    //postData.append('OrderNo', i);
    //postData.append('MerchantId', m);
    //postData.append('TerminalId', t);
    //postData.append('Encoding', 'utf-8');
    //postData.append('MerchantName', n);
    //postData.append('PayType', o);
    //postData.append('Amount', a);
    //postData.append('Mobile', p);
    //postData.append('CardHolder', c);
    //postData.append('Product', pn);
    //postData.append('Email', em);
    //postData.append('OrderDesc', item);
    //var xhr = new XMLHttpRequest();
    //xhr.open('POST', 'https://cors-anywhere.herokuapp.com/https://service.payware.com.tw/wpss/authpay.aspx');
    //xhr.send(postData);

    //var u = `https://service.payware.com.tw/wpss/authpay.aspx?OrderNo=${i}&MerchantId=${m}&TerminalId=${t}&Encoding=utf-8&MerchantName=${n}&PayType=${o}&Amount=${a}&Mobile=${p}&CardHolder=${c}&Product=${pn}&Email=${em}&OrderDesc=${item}`;
    //window.open(u, '_blank');

    // 建立訂單
    /*var postData = new FormData();
    var xhr = new XMLHttpRequest();

    postData.append("amt", $(".donateAmount").val());
    postData.append("opt", $(".donatePurpose:selected").val());

    xhr.open("POST", window.location.origin + "/Home/GoDonate", true);

    // 建成功在送金流
    xhr.onload = function () {
        if (this.status === 200) {
            if (parseInt(o) < 80) {
                i = this.responseText; // 如果有取到訂單號就直接取代隨機單號
                var u = `https://service.payware.com.tw/wpss/authpay.aspx?OrderNo=${i}&MerchantId=${m}&TerminalId=${t}&Encoding=utf-8&MerchantName=${n}&PayType=${o}&Amount=${a}&Mobile=${p}&CardHolder=${c}&Product=${pn}&Email=${em}`;
                window.open(u, '_blank');
            }
        }
    };

    xhr.send(postData);
});*/

// products = []; products[0] = "Name*1;"
function CreateOrder(
    donateType, buyerType, buyerName, buyerId, buyerPhone, buyerEmail,
    payType, donateUnit, donateUnitName, amount, donateItem, needReceipt, receiptAddr,
    pickupMethod, remark, mCode, tCode, products, installment
) {
    var postData = new FormData();
    postData.append("ot", donateType);
    postData.append("bt", buyerType);
    postData.append("bn", buyerName);
    postData.append("bi", buyerId);
    postData.append("bp", buyerPhone);
    postData.append("be", buyerEmail);
    postData.append("ps", products);
    postData.append("pt", payType);
    postData.append("du", donateUnit);
    postData.append("da", amount);
    postData.append("df", donateItem);
    postData.append("nr", needReceipt);
    postData.append("na", receiptAddr);
    postData.append("pm", pickupMethod);
    postData.append("rm", remark);

    // 1. 建單
    var xhr = new XMLHttpRequest();
    xhr.open("POST", window.location.origin + "/Home/GoDonate");
    xhr.onload = function () {
        
        if (this.status === 200 && this.responseText.indexOf("OK") > -1) {

            // 2. Call 金流
            var orderId = this.responseText.split(';')[1];
            var form = $(".fmDonate");
            form.find("#OrderNo").val(orderId);
            form.find("#MerchantId").val(mCode);
            form.find("#TerminalId").val(tCode);
            form.find("#MerchantName").val(donateUnitName);
            form.find("#PayType").val(payType);
            form.find("#Amount").val(amount);
            form.find("#Mobile").val(buyerPhone);
            form.find("#CardHolder").val(buyerName);
            form.find("#Product").val(products);
            form.find("#Email").val(buyerEmail);
            form.find("#GoBackURL").val(window.location.origin + window.location.pathname);
            form.find("#ReturnUrl").val(window.location.origin + "/Home/Return");
            form.find("#ReceiveUrl").val(window.location.origin + "/Home/Receive");
            form.find("#Installment").val(installment);
            //form.find("#OrderDesc").val(item);
            window.open('', 'new_win');
            form.submit();
        }

    };
    xhr.send(postData);
}