// Firebase SDKs のインポート
import { initializeApp } from 'https://www.gstatic.com/firebasejs/11.9.1/firebase-app.js';
// getAuth に加えて、signInWithPopup, GoogleAuthProvider, onAuthStateChanged, signOut をインポートします
import {
    getAuth,
    signInWithPopup,
    GoogleAuthProvider,
    onAuthStateChanged,
    signOut
} from 'https://www.gstatic.com/firebasejs/11.9.1/firebase-auth.js';
import { getDatabase, ref as dbRef, onValue, off } from 'https://www.gstatic.com/firebasejs/11.9.1/firebase-database.js';

// Firebase の設定
const firebaseConfig = {
    apiKey: "AIzaSyCND90Tx8Zr2z5E74NQgH09HyvwedBtONU",
    authDomain: "kokorobi-f3248.firebaseapp.com",
    databaseURL: "https://kokorobi-f3248-default-rtdb.asia-southeast1.firebasedatabase.app",
    projectId: "kokorobi-f3248",
    storageBucket: "kokorobi-f3248.firebasestorage.app",
    messagingSenderId: "74710672375",
    appId: "1:74710672375:web:b6ed67c5d06daf7c6604f2",
    measurementId: "G-LYRLRG1KQX"
};

// Firebase アプリとサービスを初期化
const app = initializeApp(firebaseConfig);
const auth = getAuth(app);
const database = getDatabase(app);

// ====== Auth Functions ======
window.authFunctions = {
    signInWithGoogle: () => {
        const provider = new GoogleAuthProvider();
        // 直接インポートした signInWithPopup 関数を呼び出し、第一引数に auth インスタンスを渡します
        return signInWithPopup(auth, provider)
            .then(result => JSON.stringify(result.user))
            .catch(error => console.error(error));
    },
    signOut: () => {
        // 直接インポートした signOut 関数を呼び出し、第一引数に auth インスタンスを渡します
        return signOut(auth);
    },
    onAuthStateChanged: (csharpCallback) => {
        // 直接インポートした onAuthStateChanged 関数を呼び出し、第一引数に auth インスタンスを渡します
        onAuthStateChanged(auth, user => {
            csharpCallback.invokeMethodAsync('OnAuthStateChanged', user ? JSON.stringify({
                uid: user.uid,
                email: user.email,
                displayName: user.displayName,
                photoUrl: user.photoURL
            }) : "");
        });
    },
    // 現在のユーザーのIDトークンを取得する関数
    // C#から呼び出されることを想定
    getIdToken: async () => {
        const user = auth.currentUser; // auth は getAuth(app) で取得した認証インスタンス
        if (user) {
            return await user.getIdToken(); // IDトークンを取得
        }
        return null; // ユーザーがログインしていない場合は null を返す
    }
};

// ====== Database Functions ======
window.dbFunctions = {
    // Realtime Database の参照を保持する変数。
    // モジュール版では `ref` 関数を使用し、`database.ref` は使用しません。
    // `dbRef` は `ref` と名前が衝突しないようにエイリアスとしています。
    _postsRef: null, // this.ref を _postsRef に変更し、プライベートな変数として扱います

    listenToPosts: (csharpCallback) => {
        // `dbRef` 関数を使って参照を作成します
        // onValue は Realtime Database のイベントリスナー関数です
        window.dbFunctions._postsRef = dbRef(database, 'posts');
        onValue(window.dbFunctions._postsRef, (snapshot) => {
            const data = snapshot.val();
            csharpCallback.invokeMethodAsync('OnPostsReceived', data ? JSON.stringify(data) : null);
        });
    },
    stopListeningToPosts: () => {
        if (window.dbFunctions._postsRef) {
            // off は Realtime Database のイベントリスナー解除関数です
            off(window.dbFunctions._postsRef, 'value');
            window.dbFunctions._postsRef = null; // 参照をクリア
        }
    }
};

// ====== Image Compression ======
window.imageUtils = {
    compressImage: async (fileStream) => {
        const arrayBuffer = await fileStream.arrayBuffer();
        const blob = new Blob([arrayBuffer]);

        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = (event) => {
                const img = new Image();
                img.onload = () => {
                    const canvas = document.createElement('canvas');
                    // 1KB以下にするために解像度をかなり下げる
                    const size = 100; // 100x100程度に
                    canvas.width = size;
                    canvas.height = size;
                    const ctx = canvas.getContext('2d');
                    ctx.drawImage(img, 0, 0, size, size);

                    // Jpegで圧縮
                    canvas.toBlob((blobResult) => {
                        const reader = new FileReader();
                        reader.onloadend = () => {
                            // Uint8Arrayを返す
                            const byteArray = new Uint8Array(reader.result);
                            resolve(byteArray);
                        };
                        reader.readAsArrayBuffer(blobResult);
                    }, 'image/jpeg', 0.5); // 品質を下げて圧縮率を上げる
                };
                img.src = event.target.result;
            };
            reader.readAsDataURL(blob);
        });
    }
};