import cv2
import tkinter as tk
from PIL import Image, ImageTk
import threading
import os

class CameraApp:
    def __init__(self, master):
        self.master = master
        self.master.title('Camera App')

        # 设置窗口大小和位置，并居中显示
        self.master.geometry('375x650+{}+{}'.format((self.master.winfo_screenwidth() - 800) // 2, (self.master.winfo_screenheight() - 600) // 2))

        # 加载背景图片
        self.bg_image = ImageTk.PhotoImage(Image.open('D:\学习\交互设计\程序前端\log1.png'))

        # 设置窗口背景为背景图片
        self.canvas = tk.Canvas(self.master, width=375, height=650, highlightthickness=0)
        self.canvas.create_image(0, 0, anchor=tk.NW, image=self.bg_image)
        self.canvas.pack()

        # 创建画布并放置在窗口左下角
        self.camera_canvas = tk.Canvas(self.master, width=200, height=220)
        self.camera_canvas.place(x=10, y=10)

        # 将画布的背景色设置为透明
        #self.camera_canvas.config(bg='SystemTransparent')

        # 创建摁键并绑定事件
        self.button = tk.Button(self.master, text='开始', font=('Arial', 24), command=self.start_camera)
        self.button.place(relx=0.5, rely=0.8, anchor=tk.CENTER)


        # 创建关闭按键并绑定事件
        self.close_button = tk.Button(self.master, text='Close', font=('Arial', 24), command=self.stop_camera)
        self.close_button.place(x=700, y=10)

        # 初始化摄像头
        self.camera = cv2.VideoCapture(0)
        #self.camera.place(x=20, y=20)
        #self.camera_canvas.place(x=20, y=20)


        # 初始化PhotoImage对象
        self.photo = None

        # 初始化视频播放状态
        self.video_playing = False

        # 初始化线程状态
        self.camera_thread_running = False
        self.video_thread_running = False

    def start_camera(self):
        self.bg_image = ImageTk.PhotoImage(Image.open('D:\学习\交互设计\程序前端\eijing2.png'))
        self.canvas.create_image(0, 10, anchor=tk.NW, image=self.bg_image)
        # 隐藏摁键
        self.button.pack_forget()

        # 显示画布
        self.camera_canvas.pack()

        # 创建线程并启动
        self.camera_thread_running = True
        self.thread = threading.Thread(target=self.show_camera)
        self.thread.start()

        # 播放视频
        if not self.video_playing:
            self.video_playing = True
            self.video_thread_running = True
            self.video_thread = threading.Thread(target=self.play_video)
            self.video_thread.start()

        # 显示摁键
        self.button.pack(pady=50)

    def stop_camera(self):
        # 停止摄像头线程
        self.camera_thread_running = False
        self.thread.join()

        # 停止视频线程
        self.video_thread_running = False
        self.video_thread.join()

        # 释放摄像头资源
        if self.camera:
            self.camera.release()

        # 释放视频资源
        if self.video_label:
            self.video_label.destroy()

        # 关闭窗口
        self.master.destroy()

    def show_camera(self):
        while self.camera_thread_running:
            # 获取摄像头的内容
            self.camera_canvas = tk.Canvas(self.master, width=320, height=240)
            self.camera_canvas.place(x=20, y=340)
            success, frame = self.camera.read()
            if not success:
                break

            # 将摄像头的内容转换为Image对象
            image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            image = Image.fromarray(image)

            # 将Image对象转换为PhotoImage对象
            if self.photo is None:
                self.photo = ImageTk.PhotoImage(image)
            else:
                self.photo.paste(image)

            # 在画布上显示PhotoImage对象
            self.camera_canvas.create_image(0, 0, anchor=tk.NW, image=self.photo)

            # 等待按键事件
            key = cv2.waitKey(1) & 0xFF
            if key == ord(' '):
                break

        # 隐藏画布
        self.camera_canvas.pack_forget()

    def play_video(self):
        # 加载视频
        video_path = os.path.abspath('D:\学习\交互设计\程序前端\shipin.mp4')
        cap = cv2.VideoCapture(video_path)

        # 获取视频的帧率和尺寸
        fps = cap.get(cv2.CAP_PROP_FPS)
        width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
        height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

        # 创建Label并放置在窗口右上角
        self.video_label = tk.Label(self.master)
        self.video_label.place(x=4, y=2)

        while self.video_thread_running:
            # 获取视频的内容
            success, frame = cap.read()
            if not success:
                break

            # 将视频的内容转换为Image对象
            image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            image = Image.fromarray(image)

            # 将Image对象转换为PhotoImage对象
            photo = ImageTk.PhotoImage(image)

            # 在Label上显示PhotoImage对象
            self.video_label.config(image=photo)
            self.video_label.image = photo

            # 等待一段时间
            delay = int(1000 / fps)
            cv2.waitKey(delay)

        # 释放视频资源
        cap.release()

    def __del__(self):
        # 释放摄像头资源
        self.camera.release()

if __name__ == '__main__':
    root = tk.Tk()
    app = CameraApp(root)

    root.mainloop()
