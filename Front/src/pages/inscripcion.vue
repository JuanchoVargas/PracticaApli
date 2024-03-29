<script setup>
import { useRouter, useRoute } from "vue-router";
import { ref, toRaw, onMounted } from "vue";
import { useCursoStore, useAuthStore } from "@/stores";
const baseUrl = import.meta.env.BASE_URL;
const router = useRouter(), route = useRoute(), storeCurso = useCursoStore(), authStore = useAuthStore();
let curso = ref(null), back = ref(null), temas = ref([]), cursosUsuario = ref([]), cursoInscrito = ref(false);

let inscribirse = async () => {
  let pd = $("#data");
  msg.confirm({
    // title: "otro",
    textCancel: "CANCELAR",
    textOk: "INSCRIBIR",
    text: `¿<span class="font-weight-semibold">${authStore.user.firstName}</span>, realmente desea inscribirse al curso <span class="font-weight-semibold">"${curso.value.nombre}"</span>?`,
    onConfirm: async () => {
      pd.lock(`Realizando inscripción,<br>un momento por favor`);
      let res = await storeCurso.inscribir({ cursoId: curso.value.id, usuarioId: authStore.user.id });
      storeCurso.cursosUsuario = []; // Limpia los cursos del usuario
      console.log("res =>", res);
      setTimeout(function () {
        pd.unlock();
        msg.success("¡Inscripción exitosa!", `<span class="font-weight-semibold d-inline-block mt-2">${authStore.user.firstName}</span>, acabamos de enviar un correo a la dirección <span class="font-weight-semibold">"${authStore.user.email}</span>" con la información de su inscripción al curso <span class="font-weight-semibold">"${curso.value.nombre}"</span>.`, function () {
          router.back();
        });
      }, 1000);
    },
    onCancel: () => { },
  });
}

onMounted(async () => {
  console.clear();
  console.log(_sep);
  console.log("inscripcion.vue mounted!");
  const id = route.params.id;
  if (id.length > 0) {
    cursoInscrito.value = await storeCurso.cursoInscrito(id);
    curso.value = await storeCurso.porId(id); // Lo carga siempre del EP
    storeCurso.curso = toRaw(curso.value);
    temas.value = await storeCurso.temasPorCursoId(id);
    back.value = curso.value.dependenciaId == 14 ? "alto-gobierno" : "capacitacion";
    console.log("curso =>", toRaw(curso.value));
    console.log("temas =>", toRaw(temas.value));
    console.log("storeCursos.curso =>", storeCurso.curso);
    $("#data").fadeIn();
  }
});
</script>
<template>
  <div class="container content">
    <div class="container content">
      <div class="card data hidden" id="data">
        <div class="card-header main d-flex justify-content-between">
          <span class="d-flex justify-content-between">
            <span>
              <span v-if="curso" class="d-flex justify-content-between">
                <span><i class="fa fa-pencil me-1"></i></span>
                <span class="ms-2">Inscripción al curso <small>[{{ curso.codigo }}]</small><br />{{
                  curso.nombre
                }}</span>
              </span>
              <span v-else>Cargando curso...</span>
            </span>
          </span>
          <span>
            <router-link :to="{ name: 'oferta', params: { por: back } }" class="btn btn-trans">
              <i class="fa-solid fa-circle-arrow-left"></i>VOLVER
            </router-link>
          </span>
        </div>
        <div class="card-body" v-if="curso">
          <div class="row">
            <div class="col-md-12">
              <div class="row p-3 bbd" id="panel-data">
                <div class="col-md-9">
                  <label class="tit">Descripción</label>
                  <p>{{ curso.descripcion }}</p>
                </div>
                <div class="col-md-3">
                  <img class="img-fluid" :src="baseUrl + 'store/img/' + curso.imagenCurso" alt="Card image cap" />
                </div>
              </div>
              <div class="row p-3 pb-0 pt-4 bbd">
                <div class="col">
                  <p class="text-center">
                    <label class="tit">Tipo de asistencia</label>
                    {{ curso.tipoAsistenciaNombre }}<br />
                    <label class="tit">Departamento</label>
                    {{ curso.departamentoNombre }}<br />
                    <label class="tit">Municipio</label> {{ curso.municipioNombre
                    }}<br />
                    <span class="me-3"><label class="tit">Cupos</label> {{ curso.cupoTotal }}</span><label
                      class="tit">Horas</label> {{
                        curso.horasTotales
                      }}<br />
                  </p>
                </div>
                <div class="col">
                  <p class="text-center">
                    <label class="tit">Lugar realización</label> {{ curso.lugarRealizacion }}<br />
                    <label class="tit">Tipo de curso</label> {{ curso.tipoCursoNombre }}<br />
                    <label class="tit">Responsable</label> {{ curso.responsable }}<br />
                    <label class="tit">Correo electrónico</label> {{ curso.correoElectronico }}<br />
                  </p>
                </div>
                <div class="col">
                  <p class="text-center">
                    <label class="tit">Inicio inscripciones</label>
                    {{ $formatDate(curso.fechaInicioInscripcion) }}<br />
                    <label class="tit">Fin inscripciones</label>
                    {{ $formatDate(curso.fechaFinInscripcion) }}<br />
                    <label class="tit">Inicio del curso</label> {{ $formatDate(curso.fechaInicio) }}
                    {{
                      $formatDate(curso.horaInicio, 'h:mm aa')
                    }}<br />
                    <label class="tit">Fin del curso</label> {{ $formatDate(curso.fechaFin) }}
                  </p>
                </div>
              </div>
              <div class="row p-3 pb-0 pt-4">
                <div class="col ps-5">
                  <p class="m-0"><label class="tit">Temas</label></p>
                  <ul class="px-3">
                    <li v-for="(item, index) in temas" :key="index">
                      {{ item.temaNombre }}
                    </li>
                  </ul>
                </div>
                <div class="col pe-5">
                  <p class="m-0"><label class="tit">Objetivos</label></p>
                  <p>{{ curso.objetivos }}</p>
                </div>
              </div>
              <div class="row p-3 pb-0 pt-4 btd" v-if="curso.documentos > 0">
                <div class="col ps-5">
                  <p class="m-0"><label class="tit">Documentos</label></p>
                  <ul class="px-3">
                    <li>
                      <a href="https://sirecec3.esap.edu.co/Areas/AdministrarSirecec/Archivos/Imagenes/FijasSirecec/Instructivo_Certificados_Sirecec.pdf"
                        target="_blank" title="Observar...">Instructivo para la descarga de los certificados del sistema
                        de registro y control de eventos de capacitación
                        <i class="fa-solid fa-arrow-up-right-from-square fa-xs ms-1"></i>
                      </a>
                    </li>
                    <li>
                      <a href="https://sirecec3.esap.edu.co/Areas/AdministrarSirecec/Archivos/Imagenes/FijasSirecec/Instructivo_Alto_Gobierno.pdf"
                        target="_blank" title="Observar...">Instructivo para la inscripción de usuarios de la escuela de
                        alto gobierno y de la dirección nacional de capacitación
                        <i class="fa-solid fa-arrow-up-right-from-square fa-xs ms-1"></i>
                      </a>
                    </li>
                  </ul>
                </div>
              </div>
              <div class="row m-0 p-0 mt-1 pt-4 btd" v-if="!authStore.user">
                <div class="col-md-12">
                  <p class="m-0 p-0 text-center font-weight-semibold"><i
                      class="fa-solid fa-circle-info me-1 color-main"></i>
                    El usuario debe encontrarse registrado y autenticado en el sistema para completar la inscripción al
                    curso. <i class="fa-solid fa-arrow-down-right color-main"></i></p>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="card-footer p-4">
          <div class="d-flex justify-content-between align-items-center">
            <span>
              <router-link :to="{ name: 'oferta', params: { por: back } }" class="btn btn-gray">
                <i class="me-1 fa-solid fa-circle-arrow-left"></i>
                VOLVER
              </router-link>
            </span>
            <span v-if="authStore.user">
              <p v-if="cursoInscrito" class="m-0 font-weight-semibold me-5"><i
                  class="fa-solid fa-circle-info me-1 color-main"></i>{{ authStore.user.firstName }}, usted ya se
                hace parte de este curso.</p>
              <span v-else>
                <a class="btn btn-main" @click.prevent="inscribirse()" v-if="curso && curso.inscripcionesAbiertas">
                  INSCRIBIRSE AL CURSO <i class="ms-1 fa-solid fa-circle-right"></i>
                </a>
                <p class="m-0 font-weight-semibold me-5" v-else><i class="fa-solid fa-circle-info me-1 color-main"></i>{{
                  authStore.user.firstName }}, lo sentimos pero los cupos para curso están agotados.</p>
              </span>
            </span>
            <span v-else>
              <router-link class="btn btn-main me-4" :to="{ name: 'registro' }">REGISTRARSE <i
                  class="fa-solid fa-user-pen ms-2"></i></router-link>
              <router-link class="btn btn-main" :to="{ name: 'ingreso' }">INGRESAR <i
                  class="fa-solid fa-user-lock ms-2"></i></router-link>
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
