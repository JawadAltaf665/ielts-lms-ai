"use client";

import { Canvas, useFrame } from "@react-three/fiber";
import { MeshDistortMaterial, Sparkles, Sphere } from "@react-three/drei";
import { useRef } from "react";
import type { Mesh } from "three";

function Orb() {
  const mesh = useRef<Mesh>(null);

  useFrame((_, delta) => {
    if (!mesh.current) return;
    mesh.current.rotation.y += delta * 0.15;
    mesh.current.rotation.x += delta * 0.04;
  });

  return (
    <Sphere ref={mesh} args={[1.4, 64, 64]}>
      <MeshDistortMaterial
        color="#7c6df2"
        emissive="#5b6dfa"
        emissiveIntensity={0.4}
        roughness={0.15}
        metalness={0.3}
        distort={0.35}
        speed={1.5}
      />
    </Sphere>
  );
}

export default function NeuralOrbCanvas() {
  return (
    <Canvas
      camera={{ position: [0, 0, 4.5], fov: 45 }}
      dpr={[1, 1.5]}
      gl={{ antialias: true, alpha: true }}
    >
      <ambientLight intensity={0.6} />
      <pointLight position={[5, 5, 5]} intensity={40} color="#4fd8e8" />
      <pointLight position={[-5, -3, -5]} intensity={20} color="#7c6df2" />
      <Orb />
      <Sparkles count={80} scale={5} size={2} speed={0.3} color="#4fd8e8" opacity={0.6} />
    </Canvas>
  );
}
