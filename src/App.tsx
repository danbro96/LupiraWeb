import { BrowserRouter, Routes, Route } from "react-router-dom";
import Layout from "./components/Layout.tsx";
import Landing from "./pages/Landing.tsx";
import About from "./pages/About.tsx";
import Projects from "./pages/Projects.tsx";
import ProjectExample from "./pages/ProjectExample.tsx";
import Kokos from "./pages/Kokos.tsx";
import Cookies from "./pages/Cookies.tsx";
import NotFound from "./pages/NotFound.tsx";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<Landing />} />
          <Route path="/about" element={<About />} />
          <Route path="/projects" element={<Projects />} />
          <Route path="/projects/example" element={<ProjectExample />} />
          <Route path="/kokos" element={<Kokos />} />
          <Route path="/cookies" element={<Cookies />} />
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
