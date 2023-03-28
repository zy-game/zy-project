layui.use(['element', 'dropdown', 'util', 'layer', 'table'], function () {
    var dropdown = layui.dropdown, util = layui.util, layer = layui.layer, table = layui.table
    window.$ = layui.jquery, element = layui.element; //Tab的切换功能，切换事件监听等，需要依赖element模块
    //监听导航点击
    element.on('nav(demo)', function (elem) {
        switchView(elem.text())
    });
    window.$(document).on('click', "#btn_search", onSearchText);
    window.$(document).on('click', "#btn_gpt", OnSendChat);
    window.$(document).on('click', "#chat_home", showChatList);
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
        url: "http://localhost:5130/api/" + apiName,
        success: function (data) {
            func(data)
        }
    })
}

function switchView(tag) {
    switch (tag) {
        case "Home":
            active("block")
            request("web/none", function (args) {
                document.getElementById("list").innerHTML = args
            })
            break
        case "Unity":
            active("block")
            request("web/unity", function (args) {
                document.getElementById("list").innerHTML = args
            })
            break
        case "C#":
            active("block")
            request("web/c#", function (args) {
                document.getElementById("list").innerHTML = args
            })
            break
        case "ChatGPT":
            active("none")
            showChatList();
            break
    }
}

function onSearchText() {
    let input = document.getElementById("tex_search").value
    if (String(input).length <= 0) {
        return
    }
    document.getElementById("tex_search").value = ""
    request("web/search/" + input, function (args) {
        document.getElementById("list").innerHTML = args

    })
}

function showChatList() {
    window.session = null
    document.getElementById("chat_session").style.display = "none"
    request("chat-gpt/list", function (args) {
        let list = document.getElementById("list")
        list.innerHTML = args
        for (i = 0; i < list.childElementCount; i++) {
            let id = list.childNodes[i].id
            list.childNodes[i].onclick = function (obj) {
                window.session = id
                request("chat-gpt/session/" + id, function (data) {
                    list.innerHTML = data
                })
            }
        }
    })
}

function OnSendChat() {
    let input = document.getElementById("chat_gpt_input").value
    if (String(input).length <= 0) {
        return
    }
    document.getElementById("chat_gpt_input").value = ''
    request("chat-gpt/" + input, function (args) {
        document.getElementById("list").innerHTML = args
    })
}
