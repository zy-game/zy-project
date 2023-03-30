layui.use(['element', 'dropdown', 'util', 'layer', 'table'], function () {
    var dropdown = layui.dropdown, util = layui.util, layer = layui.layer, table = layui.table
    $ = layui.jquery, element = layui.element; //Tab的切换功能，切换事件监听等，需要依赖element模块
    //监听导航点击
    element.on('nav(demo)', function (elem) {
        switchView(elem.text())
    });
    $(document).on('click', "#btn_search", onSearchText);
    $(document).on('click', "#btn_gpt", OnSendChat);
    $(document).on('click', "#chat_home", showChatList);
    window.onload = function () {
        switchView("Home")
    }
})
function active(state) {
    if (state == "none") {
        document.getElementById("chat_head").style.display = "block"
        document.getElementById("chat_input").style.display = "block"
        document.getElementById("search").style.display = "none"
    } else {
        document.getElementById("chat_input").style.display = "none"
        document.getElementById("chat_head").style.display = "none"
        document.getElementById("search").style.display = "block"
    }
}
function request(apiName, func) {
    window.$.ajax({
        headers: {
            session: window.session
        },
        contentType: "application/json",
        type: "GET",
        url: "http://140.143.97.63:8080/api/" + apiName,
        success: function (data) {
            func(data)
        }
    })
}

function GetTimeLineList(data) {
    let result = ''
    var dataList = JSON.parse(data)
    for (let index = 0; index < dataList.length; index++) {

        result += '<li class=\"layui-timeline-item\" id = \"' + dataList[index].uid + '\">'
        result += '<i class=\"layui-icon layui-timeline-axis\"></i>'
        result += '<div class=\"layui-timeline-content layui-text\">'
        result += '<a href = \"javascript:;\" οnclick = \"\">'
        result += '<h3 class=\"layui-timeline-title\">' + dataList[index].title + '</h3>'
        result += '</a>'
        result += '<p>' + marked.parse(dataList[index].info) + '</p>'
        result += "<ul>"
        result += "</ul>"
        result += "</div>"
        result += "</li>"
    }
    return result
}

function GetChatList(dataList) {
    let resu = ""
    for (let index = 0; index < dataList.length; index++) {
        resu += '<li class=\"layui-timeline-item\">'
        let icon = "chatgpt-icon"
        if (dataList[index].role == "Me") {
            icon = "icon"
        }
        resu += '<i class=\"layui-icon layui-timeline-axis\"><img src="' + icon + '.png" class="layui-circle" style="position:relative;width: 30px;height:30px;top:-5px;left:-5px"></i>'
        resu += '<div class=\"layui-timeline-content layui-text\">'
        resu += '<h3 class=\"layui-timeline-title\">' + dataList[index].role + '</h3>'

        resu += '<p>' + marked.parse(dataList[index].content) + '</p>'
        resu += '<ul>'
        resu += '</ul>'
        resu += '</div>'
        resu += '</li>'
    }
    return resu;
}

function ClearTimeListChilds(name) {
    let list = document.getElementById(name)
    for (i = 0; i < list.childElementCount; i++) {
        list.removeChild(list.childNodes[i])
    }
}

function switchView(tag) {
    switch (tag) {
        case "Home":
            active("block")
            request("web/none", OnGetBookListCompletion)
            break
        case "Unity":
            active("block")
            request("web/unity", OnGetBookListCompletion)
            break
        case "C#":
            active("block")
            request("web/csharp", OnGetBookListCompletion)
            break
        case "ChatGPT":
            active("none")
            showChatList();
            break
    }
}

function OnGetBookListCompletion(data) {
    ClearTimeListChilds("list")
    let list = document.getElementById("list")
    list.innerHTML = GetTimeLineList(data)
    for (i = 0; i < list.childElementCount; i++) {
        let id = list.childNodes[i].id
        list.childNodes[i].onclick = function (obj) {
            OnClickBookListItem(id)
        }
    }
}

function OnClickBookListItem(id) {
    request("web/info/" + id, function (data) {
        ClearTimeListChilds("list")
        list.innerHTML = GetChatList(data)
    })
}

function onSearchText() {
    let input = document.getElementById("tex_search").value
    if (String(input).length <= 0) {
        return
    }
    document.getElementById("tex_search").value = ""
    request("web/search/" + input, OnGetBookListCompletion)
}

function showChatList() {
    window.session = null
    ClearTimeListChilds("chat_session");
    let list = document.getElementById("list")
    request("chat-gpt", function (args) {
        ClearTimeListChilds("list");
        list.innerHTML = GetTimeLineList(args)
        for (i = 0; i < list.childElementCount; i++) {
            let id = list.childNodes[i].id
            list.childNodes[i].onclick = function (obj) {
                OnClickChatItem(id)
            }
        }
    })
}

function OnClickChatItem(id) {
    window.session = id
    request("chat-gpt/session/" + id, function (data) {
        var data = JSON.parse(data)
        ClearTimeListChilds("list");
        ClearTimeListChilds("chat_session");
        document.getElementById("chat_session").innerHTML = "<cite >" + data.title + "</cite>"
        document.getElementById("list").innerHTML = GetChatList(data.chats)
    })
}

function OnSendChat() {
    let input = document.getElementById("chat_gpt_input").value
    if (String(input).length <= 0) {
        return
    }
    $("#btn_gpt").attr("disabled", true).addClass("layui-btn-disabled")
    document.getElementById("chat_gpt_input").value = ''
    request("chat-gpt/chat/" + input, function (args) {
        ClearTimeListChilds("list");
        ClearTimeListChilds("chat_session");
        $("#btn_gpt").attr("disabled", false).removeClass("layui-btn-disabled")
        var response = JSON.parse(args)
        console.log(args)
        window.session = response.uid
        document.getElementById("chat_session").innerHTML = "<cite >" + response.title + "</cite>"
        document.getElementById("list").innerHTML = GetChatList(response.chats)
        window.scrollTo(0, document.body.scrollHeight);
    })
}
