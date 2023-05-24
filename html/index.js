let pageName = "Home"
layui.use(['element', 'dropdown', 'util', 'layer', 'table'], function () {
    var dropdown = layui.dropdown, util = layui.util, layer = layui.layer, table = layui.table
    $ = layui.jquery, element = layui.element, layedit = layui.layedit; //Tab的切换功能，切换事件监听等，需要依赖element模块
    //监听导航点击
    element.on('nav(demo)', function (elem) {
        switchView(elem.text())
    });
    $(document).on('click', "#btn_search", onSearchText);
    $(document).on('click', "#btn_gpt", OnSendChat);
    $(document).on('click', "#chat_home", showChatList);
    window.onload = function () {
        // switchView("Home")
    }
    var index = layedit.build('edit');
})
function active(state) {
    if (state == "none") {
        document.getElementById("chat_head").style.display = "block"
        document.getElementById("chat_input").style.display = "block"
        document.getElementById("search").style.display = "none"
        document.getElementById("chat_head").style.display = "block"
        document.getElementById("editor").style.display = "none"
    } else {
        document.getElementById("chat_input").style.display = "none"
        document.getElementById("chat_head").style.display = "none"
        document.getElementById("chat_head").style.display = "none"
        document.getElementById("search").style.display = "block"
        document.getElementById("editor").style.display = "none"
    }
}

let remote_url = "http://localhost:8080/"
function request(apiName, func) {
    window.$.ajax({
        headers: {
            session: window.session
        },
        contentType: "application/json",
        type: "GET",
        url: remote_url + apiName,
        dataType: 'json',
        success: function (data) {
            func(data)
        }
    })
}
function post(apiName, text, func) {
    $.ajax({
        contentType: "application/json",
        type: "POST",
        url: remote_url + apiName,
        data: text,
        dataType: "json",
        success: function (data) {
            func(data)
        }
    })
}

function GetTimeLineList(dataList) {
    let result = ''
    for (let index = 0; index < dataList.length; index++) {
        result += '<li class=\"layui-timeline-item\" id = \"' + dataList[index].uid + '\">'
        result += '<i class=\"layui-icon layui-timeline-axis\"></i>'
        result += '<div class=\"layui-timeline-content layui-text\">'
        result += '<a href = \"javascript:;\" οnclick = \"\">'
        result += '<h3 class=\"layui-timeline-title\">' + dataList[index].title + '</h3>'
        result += '</a>'
        if (dataList[index].info != null) {
            result += '<p>' + marked.parse(dataList[index].info) + '</p>'
        }
        result += "<ul>"
        result += "</ul>"
        result += "</div>"
        result += "</li>"
    }
    return result
}

function GetChatList(dataList) {
    let result = ""
    for (let index = 0; index < dataList.length; index++) {
        result += '<li class=\"layui-timeline-item\">'
        let icon = "chatgpt-icon"
        if (dataList[index].role == "Me") {
            icon = "icon"
        }
        result += '<i class=\"layui-icon layui-timeline-axis\"><img src="' + icon + '.png" class="layui-circle" style="position:relative;width: 30px;height:30px;top:-5px;left:-5px"></i>'
        result += '<div class=\"layui-timeline-content layui-text\">'
        result += '<h3 class=\"layui-timeline-title\">' + dataList[index].role + '</h3>'
        if (dataList[index].content != null) {
            result += '<p>' + marked.parse(dataList[index].content) + '</p>'
        }
        result += '<ul>'
        result += '</ul>'
        result += '</div>'
        result += '</li>'
    }
    return result;
}

function ClearTimeListChilds(name) {
    let list = document.getElementById(name)
    for (i = 0; i < list.childElementCount; i++) {
        list.removeChild(list.childNodes[i])
    }
}

function switchView(tag) {
    pageName = tag
    document.getElementById("chat_head").style.display = "none"
    document.getElementById("chat_input").style.display = "none"
    document.getElementById("search").style.display = "none"
    document.getElementById("chat_head").style.display = "none"
    switch (tag) {
        case "Home":
            ClearTimeListChilds("list")
            document.getElementById("search").style.display = "block"
            request("chat", OnGetBookListCompletion)
            break
        case "Unity":
            ClearTimeListChilds("list")
            document.getElementById("search").style.display = "block"
            request("web/unity", OnGetBookListCompletion)
            break
        case "C#":
            ClearTimeListChilds("list")
            document.getElementById("search").style.display = "block"
            request("web/csharp", OnGetBookListCompletion)
            break
        case "ChatGPT":
            ClearTimeListChilds("list")
            document.getElementById("chat_head").style.display = "block"
            document.getElementById("chat_input").style.display = "block"
            document.getElementById("chat_head").style.display = "block"
            showChatList();
            break
        case "Command":
            ClearTimeListChilds("list")
            document.getElementById("chat_input").style.display = "block"
            OnShowCommand();
            break
    }
}

function OnShowCommand() {
    request("web/cmds", function (response) {
        document.getElementById("list").innerHTML = GetChatList(response)
        window.scrollTo(0, document.body.scrollHeight);
        $("#btn_gpt").attr("disabled", false).removeClass("layui-btn-disabled")
    })
}

function OnGetBookListCompletion(data) {
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
    request("chat-gpt", function (args) {
        let list = document.getElementById("list")
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
    if (pageName == "Command") {
        post("web", '{' + '"message":"createbook","value":{"title":"Test","tag":"C#","info":"哈哈"}' + '}', function (args) {
            ClearTimeListChilds("list");
            document.getElementById("list").innerHTML = GetChatList(args)
            window.scrollTo(0, document.body.scrollHeight);
            $("#btn_gpt").attr("disabled", false).removeClass("layui-btn-disabled")
        })
        return
    }
    request("chat-gpt/chat/" + input, function (response) {
        ClearTimeListChilds("list");
        ClearTimeListChilds("chat_session");
        $("#btn_gpt").attr("disabled", false).removeClass("layui-btn-disabled")
        window.session = response.uid
        document.getElementById("chat_session").innerHTML = "<cite >" + response.title + "</cite>"
        document.getElementById("list").innerHTML = GetChatList(response.chats)
        window.scrollTo(0, document.body.scrollHeight);
    })
}
