var $;

layui.use('element', function () {
    $ = layui.jquery
        , element = layui.element; //Tab的切换功能，切换事件监听等，需要依赖element模块
    var list = document.getElementById("list")
    element.on('tab(docDemoTabBrief)', function (elem) {

        //window.alert(elem.index)
    });


    $.ajax({
        headers: {
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Methods": "PUT,POST,GET,DELETE,OPTIONS"
        },
        contentType: "application/text",
        type: "GET",
        url: "https://localhost:7167/api/web",

        success: function (data) {
            resetBodyList(data)
        }
    })

    $(document).on('click', "#search", function () {
        onSearchText(document.getElementById("search_text").value)
    });

})

function onSearchText(str) {
    console.log(str)
}

function resetBodyList(data) {
    let jsonList = JSON.parse(data)
    var result = "";
    for (i = 0; i < jsonList.length; i++) {
        temp_id = jsonList[i].id
        result += "<li class=\"layui-timeline-item\">";
        result += "<i class=\"layui-icon layui-timeline-axis\">&#xe63f; </i>";
        result += "<div class=\"layui-timeline-content layui-text\">";
        result += "<h3 class=\"layui-timeline-title\">";
        result += "<a id=\"test_" + temp_id + "\" href=\"javascript:;\" οnclick=\"onInfo(" + jsonList[i].id + ")\">" + jsonList[i].title + "</a>";
        result += "</h3><p>" + jsonList[i].simple + "</p></div ></li > ";
    }
    list.innerHTML = result
    for (i = 0; i < jsonList.length; i++) {
        let temp_id = jsonList[i].id
        $(document).on("click", "#test_" + temp_id, function () {
            onInfo(temp_id)
        })
    }
}

function onInfo(id) {
    $.ajax({
        headers: {
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Methods": "PUT,POST,GET,DELETE,OPTIONS"
        },
        contentType: "application/text",
        type: "GET",
        url: "https://localhost:7167/api/web/" + id,
        success: function (data) {
            list.innerHTML = data
        }
    })
}
