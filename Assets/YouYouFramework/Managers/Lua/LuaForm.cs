﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace YouYouFramework
{
    /// <summary>
    /// Lua组件类型
    /// </summary>
    public enum LuaComponentType
    {
        GameObject = 0,
        Transform = 1,
        Button = 2,
        Image = 3,
        YouYouImage = 4,
        Text = 5,
        YouYouText = 6,
        RawImage = 7,
        InputField = 8,
        Scrollbar = 9,
        ScrollView = 10,
        MultiScroller = 11
    }

    /// <summary>
    /// Lua窗口
    /// </summary>
    [LuaCallCSharp]
    public class LuaForm : UIFormBase
    {
        [CSharpCallLua]
        public delegate void OnInitHandler(Transform t,object userData);
        OnInitHandler onInit;

        [CSharpCallLua]
        public delegate void OnOpenHandler(object userData);
        OnOpenHandler onOpen;

        [CSharpCallLua]
        public delegate void OnCloseHandler();
        OnCloseHandler onClose;

        [CSharpCallLua]
        public delegate void OnBeforeDestroyHandler();
        OnBeforeDestroyHandler onBeforeDestroy;

        private LuaTable scriptEnv;
        private LuaEnv luaEnv;

        [Header("Lua组件")]
        [SerializeField]
        private LuaCom[] m_LuaComs;

        public LuaCom[] LuaComs
        {
            get { return m_LuaComs; }
        }

        /// <summary>
        /// 根据索引查找对应组件
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object GetLuaComs(int index)
        {
            LuaCom com = m_LuaComs[index];
            switch (com.Type)
            {
                case LuaComponentType.GameObject:
                    return com.trans.gameObject;
                case LuaComponentType.Transform:
                    return com.trans;
                case LuaComponentType.Button:
                    return com.trans.GetComponent<Button>();
                case LuaComponentType.Image:
                    return com.trans.GetComponent<Image>();
                case LuaComponentType.YouYouImage:
                    return com.trans.GetComponent<YouYouImage>();
                case LuaComponentType.Text:
                    return com.trans.GetComponent<Text>();
                case LuaComponentType.YouYouText:
                    return com.trans.GetComponent<YouYouText>();
                case LuaComponentType.RawImage:
                    return com.trans.GetComponent<RawImage>();
                case LuaComponentType.InputField:
                    return com.trans.GetComponent<InputField>();
                case LuaComponentType.Scrollbar:
                    return com.trans.GetComponent<Scrollbar>();
                case LuaComponentType.ScrollView:
                    return com.trans.GetComponent<ScrollRect>();
                case LuaComponentType.MultiScroller:
                    return com.trans.GetComponent<UIMultiScroller>();
            }
            return com.trans;
        }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            luaEnv = LuaManager.luaEnv; //此处要从LuaManager上获取 全局只有一个

            scriptEnv = luaEnv.NewTable();

            LuaTable meta = luaEnv.NewTable();
            meta.Set("__index", luaEnv.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();

            string prefabName = name;
            if (prefabName.Contains("(Clone)"))
            {
                prefabName = prefabName.Split(new string[] { "(Clone)" }, StringSplitOptions.RemoveEmptyEntries)[0] + "View";
            }

            onInit = scriptEnv.GetInPath<OnInitHandler>(prefabName + ".OnInit");
            onOpen = scriptEnv.GetInPath<OnOpenHandler>(prefabName + ".OnOpen");
            onClose = scriptEnv.GetInPath<OnCloseHandler>(prefabName + ".OnClose");
            onBeforeDestroy = scriptEnv.GetInPath<OnBeforeDestroyHandler>(prefabName + ".OnBeforeDestroy");

            scriptEnv.Set("self", this);
            if (onInit != null)
            {
                onInit(transform, userData);
            }
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (onOpen != null)
            {
                onOpen(userData);
            }
        }

        protected override void OnClose()
        {
            base.OnClose();
            if (onClose != null)
            {
                onClose();
            }
        }

        protected override void OnBeforeDestroy()
        {
            base.OnBeforeDestroy();
            if (onBeforeDestroy != null)
            {
                onBeforeDestroy();
            }
            onInit = null;
            onOpen = null;
            onClose = null;
            onBeforeDestroy = null;
        }
    }

    [Serializable]
    /// <summary>
    /// Lua组件
    /// </summary>
    public class LuaCom
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 类型
        /// </summary>
        public LuaComponentType Type;

        /// <summary>
        /// 引用
        /// </summary>
        public Transform trans;
    }
}