# Visualizing Thread Switching in .NET Async/Await

This repository provides a set of diagnostic examples to help **visualize the behavior of async/await and Task continuation in .NET**, especially in relation to **thread switching** and the **pitfalls of `.ContinueWith()`**.

🧪 Want to see how your `await` really behaves? Wondering why your continuations don’t seem to wait?  
This code helps demystify that – with good old **printf-style debugging**.

---

## 📄 Related Articles

- [The Ultimate .NET async/await Visualization Plan (just good old print debugging)](README.Ultimate.md)  
  [Japanese Version / 日本語記事はこちら](https://qiita.com/cozyupk/items/50bfa7e5ba6d6bf5121e)

- [World with a Synchronization Context](./README.World.md)  
  [Japanese Version / 日本語記事はこちら](https://qiita.com/cozyupk/items/5774e4942158fc824034)